using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Guuyb.Mq.Configs;
using Guuyb.Mq.MessageConsumerRegistration;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Guuyb.Mq
{
    internal class ConsumerBindingService<TConfig> : BackgroundService
        where TConfig : class, IConsumerBindingServiceConfig, new()
    {
        private const int RECONNECT_TIMEOUT = 1000;

        private readonly IBus _bus;
        private readonly ConsumingUnitOfWork _consumingUnitOfWork;
        private readonly IOptions<TConfig> _config;
        private readonly ILogger<ConsumerBindingService<TConfig>> _logger;
        private readonly Action<IMessageConsumerRegistration> _register;

        public ConsumerBindingService(IBus bus,
            ConsumingUnitOfWork consumingUnitOfWork,
            IOptions<TConfig> queueConfig,
            ILogger<ConsumerBindingService<TConfig>> logger,
            Action<IMessageConsumerRegistration> register)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _consumingUnitOfWork = consumingUnitOfWork ?? throw new ArgumentNullException(nameof(consumingUnitOfWork));
            _config = queueConfig ?? throw new ArgumentNullException(nameof(queueConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _register = register ?? throw new ArgumentNullException(nameof(register));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var config = _config.Value;
                    var adjustedIsNeedToDeclare = config.IsNeedToDeclare == null || config.IsNeedToDeclare.Value;
                    var queue = await DeclareQueueAsync(adjustedIsNeedToDeclare, config.QueueName, cancellationToken);
                    await DeclareExchangeWithBindingsAsync(adjustedIsNeedToDeclare, queue, config.Bindings, cancellationToken);

                    _bus.Advanced.Consume(queue,
                        (IHandlerRegistration handlerRegistration) =>
                        {
                            var messageConsumerRegistration = new MessageConsumerRegistrationForQueue(
                                    handlerRegistration,
                                    _consumingUnitOfWork);
                            _register.Invoke(messageConsumerRegistration);
                        },
                        configuration => configuration.WithPrefetchCount(config.PrefetchCount ?? 1));
                }
                catch (BrokerUnreachableException e)
                {
                    _logger.LogError(e, "Unable to establish connection with RabbitMQ");

                    // каждую секунду пытаемся поднять соединение с RabbitMQ
                    await Task.Delay(RECONNECT_TIMEOUT, cancellationToken);
                    continue;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Can't bind consumers to queue");
                    throw;
                }

                break;
            }
        }

        private async Task<IQueue> DeclareQueueAsync(bool isNeedToDeclare, string queueName, CancellationToken cancellationToken)
        {
            if (isNeedToDeclare)
            {
                return await _bus.Advanced.QueueDeclareAsync(queueName, cancellationToken);
            }

            await _bus.Advanced.QueueDeclarePassiveAsync(queueName, cancellationToken);
            return new Queue(queueName);
        }

        private async Task DeclareExchangeWithBindingsAsync(bool isNeedToDeclare, IQueue queue, IDictionary<string, string[]> bindings, CancellationToken cancellationToken)
        {
            if (bindings is null)
            {
                return;
            }

            foreach (var binding in bindings)
            {
                var exchangeName = binding.Key;
                if (!isNeedToDeclare)
                {
                    await _bus.Advanced.ExchangeDeclarePassiveAsync(exchangeName, cancellationToken);
                    continue;
                }

                var exchange = await _bus.Advanced.ExchangeDeclareAsync(
                    exchangeName,
                    configuration => configuration.WithType(ExchangeType.Topic),
                    cancellationToken);
                foreach (var routingKey in binding.Value)
                {
                    _bus.Advanced.Bind(exchange, queue, routingKey);
                }
            }
        }
    }
}
