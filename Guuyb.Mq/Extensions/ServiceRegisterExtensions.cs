using System;
using EasyNetQ;
using EasyNetQ.DI;
using Guuyb.Mq.Conventions;

namespace Guuyb.Mq.Extensions
{
    public static class ServiceRegisterExtensions
    {
        public static IServiceRegister RegisterCustomConventions(this IServiceRegister serviceRegister, Action<MapTypeNameSerializer> setup = null)
        {
            var typeNameSerializer = new MapTypeNameSerializer();
            setup?.Invoke(typeNameSerializer);

            return RegisterCustomConventions(serviceRegister, typeNameSerializer);
        }

        public static IServiceRegister RegisterCustomConventions(this IServiceRegister serviceRegister, MapTypeNameSerializer typeNameSerializer)
        {
            serviceRegister.Register<ITypeNameSerializer>(typeNameSerializer);
            serviceRegister.Register<IConventions>(c => new CustomConventions(c.Resolve<ITypeNameSerializer>()));
            return serviceRegister;
        }
    }
}
