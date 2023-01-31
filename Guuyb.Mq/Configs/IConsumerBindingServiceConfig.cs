using System.Collections.Generic;

namespace Guuyb.Mq.Configs
{
    public interface IConsumerBindingServiceConfig
    {
        string QueueName { get; set; }
        ushort? PrefetchCount { get; set; }
        bool? IsNeedToDeclare { get; set; }
        IDictionary<string, string[]> Bindings { get; set; }
    }
}
