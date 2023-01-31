using System.Collections.Generic;

namespace Guuyb.Mq.Configs
{
    public class ConsumerBindingServiceConfig : IConsumerBindingServiceConfig
    {
        public string QueueName { get; set; }
        public ushort? PrefetchCount { get; set; }
        public bool? IsNeedToDeclare { get; set; }
        public IDictionary<string, string[]> Bindings { get; set; }
    }
}
