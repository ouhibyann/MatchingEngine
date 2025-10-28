using System.Threading.Channels;

namespace MatchingEngine.Transport;

public interface IMessageBus<T>
{
    ChannelWriter<T> Writer { get; }
    ChannelReader<T> Reader { get; }
}
