namespace MatchingEngine.Loggers;

public interface IAsyncLogger
{
    ValueTask WriteLineAsync(string line, CancellationToken ct = default);
}