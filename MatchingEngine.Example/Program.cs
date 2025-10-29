using System.Diagnostics;
using System.Threading.Channels;
using MatchingEngine.Example;
using MatchingEngine.Example.Workers;
using MatchingEngine.Transport;
using Microsoft.Extensions.Configuration;

// StopWatch for "benchmarking"
Stopwatch sw = Stopwatch.StartNew();
sw.Start();

// Load Config
IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();
Config cfg = new Config()
{
    Producers = configuration.GetValue<int>("Config:Producers"),
    Consumers = configuration.GetValue<int>("Config:Consumers"),
    MessagesPerProducer = configuration.GetValue<int>("Config:MessagesPerProducer")
};

// Create Hub and Cancellation Token
CancellationTokenSource cts = new CancellationTokenSource();
Hub<Instrument> hub = new Hub<Instrument>(capacity: 128, fullMode: BoundedChannelFullMode.Wait, singleWriter: false,
    singleReader: false);

// Create AsyncLogger
var logger = new AsyncLogger(cts.Token);
await logger.StartAsync();

// Create Consumer Tasks
// Creating them before the Producer Tasks gives a chance to every consumer to be awakened equally
Task[] consumerTasks = new Task[cfg.Consumers];
for (int i = 0; i < cfg.Consumers; i++)
{
    int buyerId = i;
    var worker = new Consumers(buyerId, hub, logger);
    consumerTasks[i] = worker.RunAsync(cts.Token);
}

// Create Producer Tasks
Producer<Instrument> producer = new Producer<Instrument>(hub);
Task[] producerTasks = new Task[cfg.Producers];
for (int i = 0; i < cfg.Producers; i++)
{
    int sellerId = i;
    Producers producers = new Producers(sellerId, cfg.MessagesPerProducer, producer, hub, logger);
    producerTasks[i] = producers.RunAsync(cts.Token);
}


await Task.WhenAll(producerTasks);
hub.Writer.Complete();
await Task.WhenAll(consumerTasks);

await logger.StopAsync();
sw.Stop();
Console.WriteLine($"Example finished. Elapsed: {sw.Elapsed}");