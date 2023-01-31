using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;

namespace Guuyb.Mq
{
    /// <summary>
    /// Обертка, позволяющая через DI создавать потребителей сообщений в scope
    /// </summary>
    public class ConsumingUnitOfWork
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly ActivitySource ActivitySource = new ActivitySource(nameof(ConsumingUnitOfWork));

        public ConsumingUnitOfWork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Consume<TMessage>(TMessage message, MessageReceivedInfo messageReceivedInfo, MessageProperties properties, CancellationToken cancellationToken)
        {
            if (!ActivitySource.HasListeners())
            {
                ActivitySource.AddActivityListener(new ActivityListener()
                {
                    ShouldListenTo = _ => true,
                    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                });
            }

            var activityName = $"{messageReceivedInfo.RoutingKey} receive";
            var parentActivityId = ExtractTraceContextFromBasicProperties(properties, "ParentActivityId");

            using (var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentActivityId))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer<TMessage>>();
                    await consumer.ConsumeAsync(message, cancellationToken);
                }
            }
        }

        private string ExtractTraceContextFromBasicProperties(MessageProperties props, string key)
        {
            if (!props.Headers.TryGetValue(key, out var value))
                return null;

            var bytes = value as byte[];
            if (bytes is null)
                return null;

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
