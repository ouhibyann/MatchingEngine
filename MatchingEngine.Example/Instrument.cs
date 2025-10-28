namespace MatchingEngine.Example;

public sealed record Instrument(
    Guid Id,
    decimal Price,
    int Quantity,
    DateTime CreatedOn
) : IInstrument
{
    public static Instrument New(decimal price, int qty)
    {
        return new Instrument(Guid.NewGuid(), price, qty, DateTime.UtcNow);
    }
}