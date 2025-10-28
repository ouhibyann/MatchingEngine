using System.Diagnostics;
using System.Threading.Channels;
using MatchingEngine.Example;
using MatchingEngine.Transport;
using Microsoft.Extensions.Configuration;

// Load Config
var configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json")
    .Build();
var cfg = new Config()
{
    Producers = configuration.GetValue<int>("Config:Producers"),
    Consumers = configuration.GetValue<int>("Config:Consumers"),
    MessagesPerProducer = configuration.GetValue<int>("Config:MessagesPerProducer")
};

// Create Hub, producer, consumer
var cts = new CancellationTokenSource();
var hub = new Hub<Instrument>(capacity: 128, fullMode: BoundedChannelFullMode.Wait, singleWriter: false,
    singleReader: false);
var producer = new Producer<Instrument>(hub);
var consumer = new Consumer<Instrument>(hub);

// StopWatch for "benchmarking"
Stopwatch sw = Stopwatch.StartNew();
sw.Start();



var consumerTask = Task.Run(() => consumer.RunAsync(cts.Token));


for (int i = 1; i < 1_000_000; i++)
{
    var order = Instrument.New(price: 10 + i, qty: (1 + i) % i);
    await producer.PublishAsync(order, cts.Token);
}

hub.Writer.TryComplete();
cts.Cancel();
sw.Stop();

try
{
    await consumerTask;
}
catch (OperationCanceledException)
{
    Console.WriteLine($"Example Canceled. Elapsed: {sw.Elapsed}");
}

Console.WriteLine($"Example finished. Elapsed: {sw.Elapsed}");