# ENGIE - Simplified Matching Engine

### Please implement a simplified matching engine as a C# console application.

**Your implementation should:**<br>
Match buy and sell orders based on price-time priority —
orders with better prices are matched first, and when prices are equal, earlier orders take priority. <br>
Support concurrent access (multiple threads inserting and matching orders).<br>
Follow clean code and best practices, focusing on readability, maintainability, and performance.<br>
Use appropriate data structures for fast lookups and efficient matching.<br>
Properly handle race conditions and synchronization while minimizing locking or contention.<br>

**Attention points:**<br>
Apply high-performance techniques (e.g., object pooling, lock-free structures).
<br>
<br>

## MatchingEngine

The solution has been scoped with the following assumptions / choices in mind:

- The matching is made on a vendor / buyer principle -> There is no order-book / Trades / events.
  This choice has been made due to time constraint, prototyping, and personal assumptions.
- Instruments are generic enough to handle different kinds of tickers.
- There is currently only one instrument through the MP-MC model implemented, if more were to be added, a partition
  strategi by tickers (kafka-like design) could be implemented.

### Concurrency & performance

Bounded `Channel<T>` in `Hub.cs` with configurable FullMode and capacity enables backpressure policy.<br>
`Interlocked.Increment` on `InsertedOnTicks` ensures order within the same timestamp bucket.

## MatchingEngine—Example

## Build & Run

```shell
# Allows you to run a working example with logs
dotnet build -c Release
dotnet run --project MatchingEngine.Example -c Release
```

Edit `appsettings.json` to adjust Producers, Consumers, and MessagesPerProducer.<br>
You can also toggle the logging in `appsettings.json` - disable to see real performances as logging put a lot of
pressure on the GC
