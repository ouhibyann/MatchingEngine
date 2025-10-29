using System.Threading.Channels;

namespace MatchingEngine.Example;

public sealed class AsyncLogger
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();
    private readonly CancellationToken _outer;
    private Task? _task;

    public AsyncLogger(CancellationToken outer)
    {
        _outer = outer;
    }

    public ValueTask WriteLineAsync(string line, CancellationToken ct = default)
    {
        return _channel.Writer.WriteAsync(line, ct);
    }

    public Task StartAsync()
    {
        _task = Task.Run(async () =>
        {
            try
            {
                await foreach (var line in _channel.Reader.ReadAllAsync(_outer))
                    Console.WriteLine(line);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation cancelled");
            }
        }, _outer);
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _channel.Writer.TryComplete();
        if (_task is not null) await _task;
    }
}