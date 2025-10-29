using System.Threading.Channels;

namespace MatchingEngine.Loggers;

public sealed class AsyncLogger : IAsyncLogger
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();
    private readonly CancellationToken _outer;
    private Task? _task;
    public bool Enabled { get; set; } = true;

    public AsyncLogger(CancellationToken outer) { _outer = outer; }

    public ValueTask WriteLineAsync(string line, CancellationToken ct = default)
    {
        return Enabled ? _channel.Writer.WriteAsync(line, ct) : ValueTask.CompletedTask;
    }

    // Create a scoped logger that prefixes category
    public IAsyncLogger For(string category)
    {
        return new Scoped(this, category);
    }

    private sealed class Scoped : IAsyncLogger
    {
        private readonly AsyncLogger _root;
        private readonly string _prefix;
        public Scoped(AsyncLogger root, string category)
        {
            _root = root;
            _prefix = $"[{category}] ";
        }
        public ValueTask WriteLineAsync(string line, CancellationToken ct = default)
        {
            return _root.WriteLineAsync(_prefix + line, ct);
        }
    }

    public Task StartAsync()
    {
        if (!Enabled) return Task.CompletedTask;
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
        if (!Enabled) return;
        _channel.Writer.TryComplete();
        if (_task is not null) await _task;
    }
}