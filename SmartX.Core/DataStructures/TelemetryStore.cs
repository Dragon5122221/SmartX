using System;
using System.Collections.Generic;
using System.Linq;
using SmartX.Shared.Contracts.Telemetry;
using SmartX.Shared.Telemetry;

namespace SmartX.Core.DataStructures;

/// <summary>
/// DS-06: indexes typed sensor histories by device identifier.
/// The owning gateway synchronizes access to this store.
/// </summary>
public sealed class TelemetryStore<T>
{
    // DATA STRUCTURE DS-06:
    // Case-insensitive dictionary mapping each sensor identifier to its
    // own DS-04 custom circular batch collection.
    // Identifier lookup has O(1) average time complexity.
    private readonly Dictionary<string, TelemetryBatchBuffer<T>> _history =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Total number of packets retained across this typed store.
    /// </summary>
    public int Count => _history.Values.Sum(buffer => buffer.Count);

    /// <summary>
    /// STORE-01: append a sensor's validated sequential batch,
    /// creating its history buffer when first needed.
    /// </summary>
    public void Append(string id, TelemetryPacket<T>[] batch)
    {
        if (!_history.TryGetValue(id, out var buffer))
        {
            buffer = new TelemetryBatchBuffer<T>();
            _history[id] = buffer;
        }

        buffer.AppendBatch(batch);
    }

    /// <summary>
    /// STORE-02: return a bounded chronological history snapshot,
    /// copying only the requested recent packets from the custom buffer.
    /// </summary>
    public TelemetryHistoryResponse<T> History(
        string id,
        int take = 120)
    {
        if (!_history.TryGetValue(id, out var buffer))
        {
            return new(id, 0, []);
        }

        // Preserve the existing public history request limits.
        int requestedCount = Math.Clamp(take, 1, 512);

        // DATA STRUCTURE DS-05 USE:
        // Transfer the requested window directly from jagged batches
        // into an independent generic list.
        // BatchCount describes all batches retained for this sensor.
        return new(
            id,
            buffer.BatchCount,
            buffer.Recent(requestedCount));
    }

    /// <summary>
    /// STORE-03: retrieve the newest typed packet through an average
    /// O(1) dictionary lookup and O(1) circular-buffer access.
    /// Returns null when the sensor has no retained packet.
    /// </summary>
    public TelemetryPacket<T>? Latest(string id)
    {
        // DATA STRUCTURE DS-06 USE:
        // Access the existing sensor buffer directly instead of creating
        // a history response and temporary lists for one packet.
        return _history.TryGetValue(id, out var buffer)
            ? buffer.Latest()
            : null;
    }
}