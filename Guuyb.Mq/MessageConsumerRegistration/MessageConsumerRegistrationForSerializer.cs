using Guuyb.Mq.Conventions;
using System;

namespace Guuyb.Mq.MessageConsumerRegistration
{
    internal class MessageConsumerRegistrationForSerializer : IMessageConsumerRegistration
    {
        private readonly MapTypeNameSerializer _mapTypeNameSerializer;

        public MessageConsumerRegistrationForSerializer(MapTypeNameSerializer mapTypeNameSerializer)
        {
            _mapTypeNameSerializer = mapTypeNameSerializer ?? throw new ArgumentNullException(nameof(mapTypeNameSerializer));
        }

        public IMessageConsumerRegistration Add<TMessage, TConsumer>(string specificTypeName = null)
            where TConsumer : class, IMessageConsumer<TMessage>
        {
            _mapTypeNameSerializer.TryUse<TMessage>(specificTypeName);
            return this;
        }
    }
}
