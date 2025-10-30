namespace MatchingEngine.Tests

// Needed as NUnit needs concrete type
{
    public sealed class TestOrder : IInstrument
    {
        public Guid Id { get; }
        public Side Side { get; init; }
        public decimal Price { get; init; }
        public int Quantity { get; init; }
        public DateTime CreatedOn { get; }
        public long InsertedOnTicks { get; set; }

        public static TestOrder Buy(decimal px, int qty) => new() { Side = Side.Buy, Price = px, Quantity = qty };
        public static TestOrder Sell(decimal px, int qty) => new() { Side = Side.Sell, Price = px, Quantity = qty };
    }
}