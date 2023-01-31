using EasyNetQ;

namespace Guuyb.Mq.Conventions
{
    public class CustomConventions : EasyNetQ.Conventions
    {
        public CustomConventions(ITypeNameSerializer typeNameSerializer) : base(typeNameSerializer)
        {
            ErrorQueueNamingConvention = messageInfo => $"{messageInfo.Queue}_error";
            ErrorExchangeNamingConvention = messageInfo =>  $"{messageInfo.Queue}_error";
        }
    }
}
