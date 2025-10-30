using System.Diagnostics;
using System.Threading.Channels;
using MatchingEngine.Example;
using MatchingEngine.Example.Workers;
using MatchingEngine.Loggers;
using MatchingEngine.Transport;
using Microsoft.Extensions.Configuration;
using AsyncLogger = MatchingEngine.Loggers.AsyncLogger;

// StopWatch for "benchmarking"
Stopwatch sw = Stopwatch.StartNew();
sw.Start();

// Load Config
IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();
Config cfg = new Config()
{
    LoggingEnabled = configuration.GetValue<bool>("Logging:Enabled"),
    Producers = configuration.GetValue<int>("Config:Producers"),
    Consumers = configuration.GetValue<int>("Config:Consumers"),
    MessagesPerProducer = configuration.GetValue<int>("Config:MessagesPerProducer")
};

// Create Hub and Cancellation Token
CancellationTokenSource cts = new CancellationTokenSource();
Hub<Instrument> hub = new Hub<Instrument>(capacity: 10000, fullMode: BoundedChannelFullMode.Wait, singleWriter: false,
    singleReader: false);

// Create AsyncLogger
AsyncLogger logger = new AsyncLogger(cts.Token) { Enabled = cfg.LoggingEnabled };
await logger.StartAsync();

// Create Consumer Tasks
// Creating them before the Producer Tasks gives a chance to every consumer to be awakened equally
Task[] consumerTasks = new Task[cfg.Consumers];
for (int i = 0; i < cfg.Consumers; i++)
{
    IAsyncLogger producerLog = logger.For($"Consumer-{i}");
    Consumer<Instrument> consumer = new Consumer<Instrument>(hub, producerLog);
    Consumers worker = new Consumers(i, consumer);
    consumerTasks[i] = worker.RunAsync(cts.Token);
}

// Create Producer Tasks
Task[] producerTasks = new Task[cfg.Producers];
for (int i = 0; i < cfg.Producers; i++)
{
    IAsyncLogger producerLog = logger.For($"Producer-{i}");
    Producer<Instrument> producer = new Producer<Instrument>(hub, producerLog);
    Producers producers = new Producers(i, cfg.MessagesPerProducer, producer);
    producerTasks[i] = producers.RunAsync(cts.Token);
}


await Task.WhenAll(producerTasks);
hub.Writer.Complete();
await Task.WhenAll(consumerTasks);

await logger.StopAsync();
sw.Stop();
Console.WriteLine($"Example finished. Elapsed: {sw.Elapsed}");