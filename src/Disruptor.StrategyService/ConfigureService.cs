//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Disruptor.StrategyService.Extensions
//{
//    public static class ConfigureService
//    {
//        public static IServiceCollection AddDisruptorService([NotNull] this IServiceCollection services)
//        {
//            services.TryAdd(ServiceDescriptor.Singleton(typeof(IBatchEventHandler<>), typeof(DefaultBatchEventHandler<>));

//            return services;
//        }

//        public static IDisruptorServiceBuilder AddDisruptorService(
//            [NotNull] this IServiceCollection services,
//            [NotNull] Func<IServiceProvider, List<IBatchEventHandler>> handlerFactory)
//        {
//            services.AddDisruptorService();

//            var instances = handlerFactory(services.BuildServiceProvider());
//            var builder = DefaultBatchEventHandler();
//            builder.Instances.AddRange(instances);

//            return builder;
//        }
//    }
//}
