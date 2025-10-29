namespace MatchingEngine.Example;

public sealed class Config
{
    public bool LoggingEnabled { get; init; } = true;
    public int Producers { get; init; } = 1;
    public int Consumers { get; init; } = 1;
    public int MessagesPerProducer { get; init; } = 1000;
}