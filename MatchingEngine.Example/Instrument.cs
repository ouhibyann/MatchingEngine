namespace MatchingEngine.Example;

public sealed record Instrument(
    Guid Id,
    decimal Price,
    int Quantity,
    DateTime CreatedOn,
    Side Side
) : IInstrument
{
    public long InsertedOnTicks { get; set; } // stamped by Producer

    public static Instrument New(decimal price, int qty, Side side)
    {
        return new Instrument(Guid.NewGuid(), price, qty, DateTime.UtcNow, side) { InsertedOnTicks = 0 };
    }
}