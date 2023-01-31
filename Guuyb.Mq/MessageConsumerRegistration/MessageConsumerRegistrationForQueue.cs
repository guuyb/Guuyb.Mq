using EasyNetQ;
using EasyNetQ.Consumer;
using System;

namespace Guuyb.Mq.MessageConsumerRegistration
{
    internal class MessageConsumerRegistrationForQueue : IMessageConsumerRegistration
    {
        private readonly IHandlerRegistration _handlerRegistration;
        private readonly ConsumingUnitOfWork _consumingUnitOfWork;

        public MessageConsumerRegistrationForQueue(
            IHandlerRegistration handlerRegistration,
            ConsumingUnitOfWork consumingUnitOfWork)
        {
            _handlerRegistration = handlerRegistration ?? throw new ArgumentNullException(nameof(handlerRegistration));
            _consumingUnitOfWork = consumingUnitOfWork ?? throw new ArgumentNullException(nameof(consumingUnitOfWork));
        }

        public IMessageConsumerRegistration Add<TMessage, TConsumer>(string specificTypeName = null)
            where TConsumer : class, IMessageConsumer<TMessage>
        {
            _handlerRegistration.Add<TMessage>((message, info, cancellationToken) =>
                _consumingUnitOfWork.Consume(message.Body, info, message.Properties, cancellationToken));
            return this;
        }
    }
}
