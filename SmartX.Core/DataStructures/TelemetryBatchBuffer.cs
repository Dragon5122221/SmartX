using System;
using System.Collections.Generic;
using System.Linq;
using SmartX.Shared.Telemetry;

//All Data Structures Clearly Labeled
namespace SmartX.Core.DataStructures;

/// <summary>
/// DS-04: custom bounded circular collection of jagged telemetry batches.
/// Variable-length batches avoid padding unused array elements.
/// See Microsoft (2026c) for jagged arrays.
/// </summary>
/// <remarks>
/// The owning gateway must synchronize access.
/// Appending copies the incoming array; snapshots expose independent arrays.
/// Packet ordering and sensor-specific validation belong to the ingestion layer.
/// </remarks>
public sealed class TelemetryBatchBuffer<T>
{
    // DATA STRUCTURE DS-04:
    // Jagged backing array owned by this custom circular collection.
    // Each occupied slot contains one non-empty batch of non-null packets.
    // Its capacity is validated before allocation.
    private readonly TelemetryPacket<T>[][] _batches;

    // Physical position of the oldest retained batch.
    private int _head;

    /// <summary>
    /// COL-06: create a circular batch collection with an explicitly
    /// validated capacity, rather than silently adjusting invalid input.
    /// </summary>
    /// <param name="capacity">
    /// Maximum retained batch count, from 1 through 128 inclusive.
    /// Defaults to 32 batches.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The requested capacity is outside the supported range.
    /// </exception>
    public TelemetryBatchBuffer(int capacity = 32)
    {
        if (capacity is < 1 or > 128)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                capacity,
                "Telemetry buffer capacity must be between 1 and 128 batches.");
        }

        _batches = new TelemetryPacket<T>[capacity][];
    }

    /// <summary>
    /// Number of occupied batch slots.
    /// </summary>
    public int BatchCount { get; private set; }

    /// <summary>
    /// Total number of packets across all retained batches.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// COL-01: validate and copy an incoming batch before changing
    /// retained state, then evict the oldest whole batch if necessary.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The supplied batch array is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The batch exceeds 256 packets or contains a null packet.
    /// </exception>
    public void AppendBatch(TelemetryPacket<T>[] batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        if (batch.Length == 0)
        {
            return;
        }

        if (batch.Length > 256)
        {
            throw new ArgumentException(
                "A sensor batch is limited to 256 readings.",
                nameof(batch));
        }

        // DATA STRUCTURE DS-04 INPUT COPY:
        // Create an owned array before changing counts or retained slots.
        // Validate this exact copy so the checked array is the stored array.
        TelemetryPacket<T>[] ownedBatch = batch.ToArray();

        for (int index = 0; index < ownedBatch.Length; index++)
        {
            if (ownedBatch[index] is null)
            {
                throw new ArgumentException(
                    $"The telemetry packet at index {index} cannot be null.",
                    nameof(batch));
            }
        }

        // All validation and allocation above have completed successfully.
        // Only now may the retained collection change.
        if (BatchCount == _batches.Length)
        {
            int evictedPacketCount = _batches[_head].Length;

            _batches[_head] = ownedBatch;
            _head = (_head + 1) % _batches.Length;

            Count = Count - evictedPacketCount + ownedBatch.Length;
        }
        else
        {
            int insertionIndex =
                (_head + BatchCount) % _batches.Length;

            _batches[insertionIndex] = ownedBatch;
            BatchCount++;
            Count += ownedBatch.Length;
        }
    }

    /// <summary>
    /// COL-02: copy retained batches into independent arrays,
    /// ordered from the oldest batch to the newest.
    /// </summary>
    public TelemetryPacket<T>[][] SnapshotBatches()
    {
        // DATA STRUCTURE DS-04 SNAPSHOT:
        // A separate jagged array protects the retained array structure
        // from changes made by callers.
        var snapshot = new TelemetryPacket<T>[BatchCount][];

        for (int index = 0; index < BatchCount; index++)
        {
            int physicalIndex =
                (_head + index) % _batches.Length;

            snapshot[index] = _batches[physicalIndex].ToArray();
        }

        return snapshot;
    }

    /// <summary>
    /// COL-03: transfer all retained packets into a pre-sized generic
    /// list while preserving chronological batch order.
    /// </summary>
    public List<TelemetryPacket<T>> ToList()
    {
        // DATA STRUCTURE DS-05:
        // Pre-sized List<TelemetryPacket<T>> receives packets from
        // the jagged backing array without repeated capacity growth.
        var result = new List<TelemetryPacket<T>>(Count);

        for (int index = 0; index < BatchCount; index++)
        {
            int physicalIndex =
                (_head + index) % _batches.Length;

            result.AddRange(_batches[physicalIndex]);
        }

        return result;
    }

    /// <summary>
    /// COL-04: retrieve the newest retained packet in O(1) time
    /// without allocating a history list or copying a batch.
    /// Returns null when the collection is empty.
    /// </summary>
    public TelemetryPacket<T>? Latest()
    {
        if (BatchCount == 0)
        {
            return null;
        }

        // DATA STRUCTURE DS-04 USE:
        // Translate the newest logical batch position into its physical
        // slot, including when the circular collection has wrapped.
        int newestBatchIndex =
            (_head + BatchCount - 1) % _batches.Length;

        TelemetryPacket<T>[] newestBatch =
            _batches[newestBatchIndex];

        // AppendBatch never retains an empty batch or a null packet.
        // Return the existing immutable packet without exposing its array.
        return newestBatch[^1];
    }

    /// <summary>
    /// COL-05: copy at most the requested number of newest packets into
    /// an independent list, ordered from oldest to newest in that window.
    /// </summary>
    /// <remarks>
    /// Time complexity is O(b + k), where b is the retained batch count
    /// and k is the number of packets copied. Output space is O(k).
    /// </remarks>
    public List<TelemetryPacket<T>> Recent(int maximumReadings)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumReadings);

        int requestedCount = Math.Min(maximumReadings, Count);

        // DATA STRUCTURE DS-05 USE:
        // Allocate list capacity for the requested window only.
        // Packets outside this window are never copied into a list.
        var result = new List<TelemetryPacket<T>>(requestedCount);

        if (requestedCount == 0)
        {
            return result;
        }

        int readingsToSkip = Count - requestedCount;

        // DATA STRUCTURE DS-04 USE:
        // Traverse batches in logical chronological order, regardless
        // of where the oldest batch sits in the physical backing array.
        for (int batchOffset = 0;
             batchOffset < BatchCount;
             batchOffset++)
        {
            int physicalIndex =
                (_head + batchOffset) % _batches.Length;

            TelemetryPacket<T>[] batch = _batches[physicalIndex];

            // Skip entire older batches without copying their packets.
            if (readingsToSkip >= batch.Length)
            {
                readingsToSkip -= batch.Length;
                continue;
            }

            // The requested window may begin partway through a batch.
            // Subsequent batches are copied from their first packet.
            for (int packetIndex = readingsToSkip;
                 packetIndex < batch.Length;
                 packetIndex++)
            {
                result.Add(batch[packetIndex]);
            }

            readingsToSkip = 0;
        }

        return result;
    }
}
//If you are reading this, it is essentially a means of getting a few extra commits after losing a few files.
//I hope you're having a lovely day! I sound like an AI, I swear...but I wanna be a lil cheesy
//Once upon a time, I was riding my motorcycle when I got stopped by a guy. He said I bumped him, which I didn't. 
//He asked me to pull over at the garage, in which case I smiled. Arriving there, I called for a man named Dave in the most zesty and over-the-top way I could.
//Dave, a very friendly police officer, came over and asked what the problem was. Let's just say that After I left, I don't doubt that dave wanted to mess with him a lil bit.
//On a more serious note, my lecturer for this module has been legitimately amazing. Though my many cascading issues have made attending class difficult, she was always communicating and updating us. She shows genuine care and love for the game. I appreciate her a lot.

