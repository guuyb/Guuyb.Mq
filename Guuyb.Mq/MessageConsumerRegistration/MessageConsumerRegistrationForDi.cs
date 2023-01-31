using Microsoft.Extensions.DependencyInjection;
using System;

namespace Guuyb.Mq.MessageConsumerRegistration
{
    internal class MessageConsumerRegistrationForDi : IMessageConsumerRegistration
    {
        private readonly IServiceCollection _services;

        public MessageConsumerRegistrationForDi(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IMessageConsumerRegistration Add<TMessage, TConsumer>(string specificTypeName = null) 
            where TConsumer : class, IMessageConsumer<TMessage>
        {
            _services.AddTransient<IMessageConsumer<TMessage>, TConsumer>();
            return this;
        }
    }
}
