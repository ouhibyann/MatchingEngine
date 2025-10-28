using System.Diagnostics;
using System.Threading.Channels;
using MatchingEngine.Example;
using MatchingEngine.Transport; 

var cts = new CancellationTokenSource();
var hub = new Hub<Instrument>(capacity: 128, fullMode: BoundedChannelFullMode.Wait, singleWriter: false, singleReader: false);
var producer = new Producer<Instrument>(hub);
var consumer = new Consumer<Instrument>(hub);

Stopwatch sw = Stopwatch.StartNew();
sw.Start();

var consumerTask = Task.Run(() => consumer.RunAsync(cts.Token));


for (int i = 1; i < 1000; i++)
{
    var order = Instrument.New(price: 10 + i, qty: (1 + i) % i);
    await producer.PublishAsync(order, cts.Token);
}

hub.Writer.TryComplete();
cts.Cancel();
sw.Stop();

try { await consumerTask; }
catch (OperationCanceledException) { Console.WriteLine($"Example Canceled. Elapsed: {sw.Elapsed}"); }

Console.WriteLine($"Example finished. Elapsed: {sw.Elapsed}");