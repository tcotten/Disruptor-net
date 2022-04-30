using Disruptor;
using Disruptor.Dsl;
using Disruptor.StrategyService;
using Disruptor.StrategyService.EventConsumers;
using Disruptor.StrategyService.Models;
using Neo4jClient;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IBoltGraphClient>(context =>
        {
            var boltGraphClient = new BoltGraphClient(new Uri("neo4j://192.168.1.60:7687"), "neo4j", "Avalanch3");
            boltGraphClient.ConnectAsync().GetAwaiter();
            return boltGraphClient;
        });
        services.AddSingleton<IJournalConsumer, JournalConsumer>();
        services.AddSingleton<IReplicationConsumer, ReplicationConsumer>();
        services.AddSingleton<IStepGridStrategy, StepGridStrategy>();
        services.AddSingleton<Disruptor<TickerEvent>>(provider =>
        {
            var journalConsumer = provider.GetRequiredService<IJournalConsumer>();
            var replicationConsumer = provider.GetRequiredService<IReplicationConsumer>();
            var stepGridStrategy = provider.GetRequiredService<IStepGridStrategy>();
            var disruptor = new Disruptor<TickerEvent>(() => new TickerEvent(), 33554432);
            disruptor.HandleEventsWith(replicationConsumer, journalConsumer).Then(stepGridStrategy);

            return disruptor;
        });
        services.AddSingleton(provider =>
        {
            var disruptor = provider.GetRequiredService<Disruptor<TickerEvent>>();
            return new DisruptorExecution(disruptor);
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
