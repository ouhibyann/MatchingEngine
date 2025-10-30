using MatchingEngine.OrderBook;
using NUnit.Framework;

namespace MatchingEngine.Tests;

[TestFixture]
public class OrderBookTests
{
    private OrderBook<TestOrder> _book = null!;

    [SetUp]
    public void SetUp()
    {
        _book = new OrderBook<TestOrder>();
    }

    [Test]
    public void EnqueueBid_WhenNoAsks_TopShowsBestBidOnly()
    {
        _book.ProcessFok(TestOrder.Buy(100m, 10));

        var (bb, ba) = _book.TopOfBook();
        
        Assert.That(bb, Is.EqualTo(100m));
        Assert.That(ba, Is.Null);
    }

    [Test]
    public void EnqueueAsk_WhenNoBids_TopShowsBestAskOnly()
    {
        _book.ProcessFok(TestOrder.Sell(101m, 5));

        var (bb, ba) = _book.TopOfBook();
        
        Assert.That(bb, Is.Null);
        Assert.That(ba, Is.EqualTo(101m));
    }

    [Test]
    public void FullFill_BuyConsumesSingleAskLevel_Completely()
    {
        
        _book.ProcessFok(TestOrder.Sell(99m, 150));
        _book.ProcessFok(TestOrder.Buy(100m, 150));

        var (bb, ba) = _book.TopOfBook();
        Assert.That(bb, Is.Null);
        Assert.That(ba, Is.Null);
    }

    [Test]
    public void FokBehavior_InsufficientLiquidity_RestsIncomingOnOwnSide()
    {
        _book.ProcessFok(TestOrder.Sell(100m, 50));
        _book.ProcessFok(TestOrder.Buy(100m, 60));

        var (bb, ba) = _book.TopOfBook();
        Assert.That(bb, Is.EqualTo(100m)); 
        Assert.That(ba, Is.EqualTo(100m)); 
    }

    [Test]
    public void PricePriority_BuyConsumesBestPricedAsksFirst()
    {
        
        _book.ProcessFok(TestOrder.Sell(98m, 10));
        _book.ProcessFok(TestOrder.Sell(100m, 10));

        _book.ProcessFok(TestOrder.Buy(100m, 10));

        var (bb, ba) = _book.TopOfBook();
        Assert.That(bb, Is.Null);
        Assert.That(ba, Is.EqualTo(100m));
    }

    [Test]
    public void SymmetricFullFill_SellConsumesBidLevel_Completely()
    {
        _book.ProcessFok(TestOrder.Buy(100m, 20));
        _book.ProcessFok(TestOrder.Sell(99m, 20));

        var (bb, ba) = _book.TopOfBook();
        Assert.That(bb, Is.Null);
        Assert.That(ba, Is.Null);
    }
}