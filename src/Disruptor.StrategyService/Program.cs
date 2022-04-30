using Disruptor.StrategyService;
using Neo4jClient;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(provider => new DisruptorExecution(33554432));
        services.AddSingleton<IGraphClient>(context =>
        {
            var graphClient = new GraphClient(new Uri("http://172.2.96.1:7474"), "Neo4j", "Avalanch3");
            graphClient.ConnectAsync().GetAwaiter();
            return graphClient;
        });
        //services.AddSingleton<IGraphClientFactory>(new GraphClientFactory())
        //.Configure<NeoServerConfiguration>(NeoServerConfiguration.GetConfigurationAsync(new Uri("http://172.2.96.1:7474"), "Neo4j", "Avalanch3"));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

//static void StrategyBuilder(IServiceProvider services)
//{
//    using IServiceScope serviceScope = services.GetService<IServiceScope>();
//}
