using System.Diagnostics.Metrics;
using System.Threading.Channels;
using MatchingEngine.Loggers;
using MatchingEngine.Transport;
using NUnit.Framework;

namespace MatchingEngine.Tests;

public class TransportTests
{
    [Test]
    public async Task Producer_Assigns_Monotonic_InsertedOnTicks()
    {
        using var cts = new CancellationTokenSource(5_000);

        var hub = new Hub<Instrument>(
            capacity: 1024,
            fullMode: BoundedChannelFullMode.Wait,
            singleWriter: false,
            singleReader: false);

        var logger = new AsyncLogger(cts.Token) { Enabled = false };
        await logger.StartAsync();

        var producer = new Producer<IInstrument>(hub, logger);

        const int N = 1_000;
        for (int i = 0; i < N; i++)
            await producer.PublishAsync(new Instrument { Price = 100 + i, Quantity = 1 }, cts.Token);

        long last = 0;
        int received = 0;

        while (received < N && await hub.Reader.WaitToReadAsync(cts.Token))
        {
            while (hub.Reader.TryRead(out var msg))
            {
                Assert.That(msg.InsertedOnTicks, Is.GreaterThan(last), "InsertedOnTicks must strictly increase");
                last = msg.InsertedOnTicks;
                received++;
            }
        }

        Assert.That(received, Is.EqualTo(N));
        await logger.StopAsync();
    }

    [Test]
    public async Task MultiProducers_SingleConsumer_RoundTrips_AllMessages()
    {
        using var cts = new CancellationTokenSource(10_000);

        var hub = new Hub<Instrument>(
            capacity: 4096,
            fullMode: BoundedChannelFullMode.Wait,
            singleWriter: false,
            singleReader: false);

        var logger = new AsyncLogger(cts.Token) { Enabled = false };
        await logger.StartAsync();

        var producer = new Producer<Instrument>(hub, logger);

        const int producers = 4;
        const int perProducer = 5_000;
        var total = producers * perProducer;

        var writeTasks = Enumerable.Range(0, producers).Select(async _ =>
        {
            for (int i = 0; i < perProducer; i++)
                await producer.PublishAsync(new Instrument { Price = 100m, Quantity = 1 }, cts.Token);
        }).ToArray();

        var readCount = 0;
        var readerTask = Task.Run(async () =>
        {
            while (readCount < total && await hub.Reader.WaitToReadAsync(cts.Token))
            {
                while (hub.Reader.TryRead(out _))
                    Interlocked.Increment(ref readCount);
            }
        }, cts.Token);

        await Task.WhenAll(writeTasks);

        // No explicit Complete() on Hub, so wait until we've read them all
        while (readCount < total && !cts.IsCancellationRequested)
            await Task.Delay(10, cts.Token);

        Assert.That(readCount, Is.EqualTo(total));

        await logger.StopAsync();
    }
}
