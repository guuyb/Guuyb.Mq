using System.Threading;
using System.Threading.Tasks;

namespace Guuyb.Mq
{
    public interface IMessageConsumer<TMessage>
    {
        Task ConsumeAsync(TMessage message, CancellationToken cancellationToken);
    }
}
