using SmartX.Shared.Realtime;
namespace SmartXWebApi.Services;

/// <summary>Single consumer, bounded ingress. The heap always wins over the FIFO at the next dequeue.
/// Work already executing is not preempted. Equal critical priorities retain arrival order.</summary>
public sealed class TelemetryDispatcher : BackgroundService
{
    private sealed record Work(Func<IngestionResult> Apply, TaskCompletionSource<IngestionResult> Completion, long Arrival, DateTimeOffset Enqueued);
    private readonly object _sync = new();
    // DS-P2-01 / DS-P2-02: real processing queues, not display-only demonstrations.
    private readonly Queue<Work> _normal = new();
    private readonly PriorityQueue<Work, (int Severity, long Arrival)> _urgent = new();
    private readonly SemaphoreSlim _ready = new(0);
    private long _arrival, _standardCount, _criticalCount, _rejected;
    private double _latency;
    private bool _stopped;

    public QueueStatus Status() { lock (_sync) return new(_normal.Count, _urgent.Count, _standardCount, _criticalCount, _rejected, _latency); }

    public async Task<IngestionResult> Submit<T>(TelemetryBatchRequest<T> input, Func<T, bool> critical,
        Func<TelemetryBatchRequest<T>, IngestionResult> ingest, CancellationToken token)
    {
        if (input.Batches is null || input.Batches.Length is < 1 or > 64 || input.Batches.Any(b => b is null || b.Length > 4096) || input.Batches.Sum(b => b.Length) > 8192)
            throw new GatewayException("Use 1–64 batches, at most 4,096 packets per batch and 8,192 per request.");
        // Complete packets independently so a critical packet inside a mixed batch can bypass normal work.
        var tasks = new List<Task<IngestionResult>>();
        lock (_sync)
        {
            int size = input.Batches.Sum(b => b.Length);
            if (_stopped) throw new GatewayException("Processor is stopping; retry shortly.", 503);
            if (_normal.Count + _urgent.Count + size > 16384) { _rejected += size; throw new GatewayException("Ingress capacity reached. Retry after the queue drains.", 429); }
            foreach (var batch in input.Batches)
            foreach (var packet in batch)
            {
                var completion = new TaskCompletionSource<IngestionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                var work = new Work(() => ingest(new() { Batches = [[packet!]] }), completion, ++_arrival, DateTimeOffset.UtcNow);
                if (packet is not null && critical(packet.Value)) _urgent.Enqueue(work, (0, work.Arrival));
                else _normal.Enqueue(work);
                tasks.Add(completion.Task);
                _ready.Release();
            }
        }
        // Once accepted, processing survives a client disconnect; only that client's wait is cancelled.
        var results = await Task.WhenAll(tasks).WaitAsync(token);
        return new(results.Sum(r => r.Accepted), results.Sum(r => r.Rejected), results.Sum(r => r.Milliseconds),
            results.SelectMany(r => r.Errors).Take(100).ToList());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                await _ready.WaitAsync(stoppingToken);
                Work work; bool priority;
                lock (_sync)
                {
                    priority = _urgent.TryDequeue(out var next, out _);
                    work = priority ? next! : _normal.Dequeue();
                }
                try
                {
                    var result = work.Apply();
                    lock (_sync) { if (priority) _criticalCount++; else _standardCount++; _latency = (DateTimeOffset.UtcNow - work.Enqueued).TotalMilliseconds; }
                    work.Completion.TrySetResult(result);
                }
                catch (Exception ex) { work.Completion.TrySetException(ex); }
                // Yield periodically so network threads can enqueue new urgent packets during a burst.
                if ((_standardCount + _criticalCount) % 32 == 0) await Task.Delay(1, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            lock (_sync)
            {
                _stopped = true;
                while (_normal.TryDequeue(out var item)) item.Completion.TrySetException(new GatewayException("Processor stopped.",503));
                while (_urgent.TryDequeue(out var item, out _)) item.Completion.TrySetException(new GatewayException("Processor stopped.",503));
            }
        }
    }
}
