namespace MatchingEngine;

public enum Side { Buy, Sell }
public interface IInstrument
{
    public Guid Id { get; }
    public decimal Price { get; }
    public int Quantity { get; }
    public DateTime CreatedOn { get; }
    public long InsertedOnTicks { get; set; }
    public Side Side { get; }
}