namespace Guuyb.Mq.Configs
{
    public class RabbitMqConfig
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public ushort? Port { get; set; }
        public ushort? PrefetchCount { get; set; }
        public bool PublisherConfirms { get; set; }
        public string VirtualHost { get; set; }
    }
}
