namespace MatchingEngine;

public interface IInstrument
{
    public Guid Id { get; }
    public decimal Price { get; }
    public int Quantity { get; }
    public DateTime CreatedOn { get; }
    public long InsertedOnTicks { get; set; }
}