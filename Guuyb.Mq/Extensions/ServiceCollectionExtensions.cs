using System;
using System.Collections.Generic;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Guuyb.Mq.Configs;
using Guuyb.Mq.Conventions;
using Guuyb.Mq.MessageConsumerRegistration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Guuyb.Mq.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterMq(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<MapTypeNameSerializer> setupSerializer = null)
        {
            var config = new RabbitMqConfig();
            configuration.GetSection(nameof(RabbitMqConfig)).Bind(config);

            var connectionConfiguration = new ConnectionConfiguration();
            if (config.Port != null)
                connectionConfiguration.Port = config.Port.Value;

            if (config.UserName != null)
                connectionConfiguration.UserName = config.UserName;

            if (config.Password != null)
                connectionConfiguration.Password = config.Password;

            if (config.VirtualHost != null)
                connectionConfiguration.VirtualHost = config.VirtualHost;

            if (config.PrefetchCount != null)
                connectionConfiguration.PrefetchCount = config.PrefetchCount.Value;

            connectionConfiguration.PublisherConfirms = config.PublisherConfirms;

            if (config.Host != null)
            {
                var host = new HostConfiguration();
                host.Host = config.Host;

                if (config.Port != null)
                    host.Port = config.Port.Value;

                connectionConfiguration.Hosts = new List<HostConfiguration> { host };
            }

            var mapTypeNameSerializer = new MapTypeNameSerializer();
            if (setupSerializer != null)
            {
                setupSerializer(mapTypeNameSerializer);
            }
            return services
                .AddSingleton<ConsumingUnitOfWork>()
                .AddSingleton(mapTypeNameSerializer)
                .RegisterEasyNetQ(
                    _ => connectionConfiguration,
                    serviceRegister => serviceRegister.RegisterCustomConventions(mapTypeNameSerializer));
        }

        public static IServiceCollection BindConsumers<TConfig>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IMessageConsumerRegistration> register)
            where TConfig : class, IConsumerBindingServiceConfig, new()
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<TConfig>(configuration.GetSection(typeof(TConfig).Name));

            var messageConsumerRegistrationForDi = new MessageConsumerRegistrationForDi(services);
            register.Invoke(messageConsumerRegistrationForDi);

            return services
                .AddHostedService(sp =>
                {
                    var bus = sp.GetRequiredService<IBus>();
                    var consumingUnitOfWork = sp.GetRequiredService<ConsumingUnitOfWork>();
                    var queueConfig = sp.GetRequiredService<IOptions<TConfig>>();
                    var logger = sp.GetRequiredService<ILogger<ConsumerBindingService<TConfig>>>();
                    var serializer = sp.GetService<MapTypeNameSerializer>();

                    if (serializer != null)
                    {
                        var messageConsumerRegistrationForSerializer = new MessageConsumerRegistrationForSerializer(serializer);
                        register(messageConsumerRegistrationForSerializer);
                    }

                    return new ConsumerBindingService<TConfig>(
                        bus,
                        consumingUnitOfWork,
                        queueConfig,
                        logger,
                        register);
                });
        }
    }
}
