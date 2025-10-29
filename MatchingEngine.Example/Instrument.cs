namespace MatchingEngine.Example;

public sealed record Instrument(
    Guid Id,
    decimal Price,
    int Quantity,
    DateTime CreatedOn
) : IInstrument
{
    public long InsertedOnTicks { get; set; } // stamped by Producer

    public static Instrument New(decimal price, int qty)
    {
        return new Instrument(Guid.NewGuid(), price, qty, DateTime.UtcNow) { InsertedOnTicks = 0 };
    }
}