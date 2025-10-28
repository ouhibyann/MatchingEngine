using System.Threading.Channels;

namespace MatchingEngine.Transport;

public sealed class Hub<T>: IMessageBus<T>
{
    private readonly Channel<T> _channel;

    public Hub(int capacity = 1024,
        BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait,
        bool singleWriter = false,
        bool singleReader = false)
    {
        
        var opts = new BoundedChannelOptions(capacity)
        {
            FullMode = fullMode,
            SingleWriter = singleWriter,
            SingleReader = singleReader
        };
        _channel = Channel.CreateBounded<T>(opts);
    }

    public ChannelWriter<T> Writer => _channel.Writer;
    public ChannelReader<T> Reader => _channel.Reader;
}