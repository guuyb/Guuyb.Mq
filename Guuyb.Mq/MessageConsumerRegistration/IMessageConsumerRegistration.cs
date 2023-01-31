namespace Guuyb.Mq.MessageConsumerRegistration
{
    public interface IMessageConsumerRegistration
    {
        IMessageConsumerRegistration Add<TMessage, TConsumer>(string specificTypeName = null)
            where TConsumer : class, IMessageConsumer<TMessage>;
    }
}
