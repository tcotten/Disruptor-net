using Disruptor.Dsl;
using Disruptor.StrategyService.EventConsumers;
using Disruptor.StrategyService.Models;
using Disruptor.StrategyService.Producers;
using Neo4jClient;

namespace Disruptor.StrategyService
{
    public class Worker : BackgroundService
    {
        private readonly DisruptorExecution _disruptorService;
        private readonly ILogger<Worker> _logger;

        public Worker(DisruptorExecution disruptor, ILogger<Worker> logger)
        {
            _disruptorService = disruptor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //var disruptorExecutor = new DisruptorExecution(33554432);
            await _disruptorService.StartDisruptor();
            while (!stoppingToken.IsCancellationRequested)
            {
                //lock (disruptorExecutor)
                //{
                //    if (!disruptorExecutor.IsStarted)
                //    {
                //        disruptorExecutor.StartDisruptor().GetAwaiter().GetResult();
                //    }
                //}
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
            await _disruptorService.StopDisruptor();
        }
    }

    public class DisruptorExecution
    {
        private Disruptor<PairCandle> disruptor;
        public bool IsStarted { get { return disruptor != null && disruptor.HasStarted; } }
        public DisruptorExecution(Disruptor<PairCandle> disruptor)
        {
            this.disruptor = disruptor;
        }

        public async Task StartDisruptor()
        {
            await Task.Run(() =>
            {
                //disruptor = new Disruptor<TickerEvent>(() => new TickerEvent(), ringBufferSize);

                //disruptor.HandleEventsWith(new ReplicationConsumer()).Then(new JournalConsumer()).Then(new TickerEventHandler());

                //disruptor.HandleEventsWith(new ReplicationConsumer(), new JournalConsumer()).Then(new TickerEventHandler());

                //disruptor.HandleEventsWith(new ReplicationConsumer(), new JournalConsumer()).Then(new StepGridStrategy());

                var producer = new TickerProducer(@"C:\Users\tcott\OneDrive\Apps\gunbot\Disruptor-net\src\Disruptor.MySamples\Data\ETHUSD_1.csv");

                if (null != disruptor)
                {
                    disruptor.Start();

                    producer.ProduceEvents(disruptor.PublishTickerEvents);
                }


            });
            //await Task.FromResult(Task.CompletedTask);
        }

        public async Task StopDisruptor()
        {
            if (disruptor != null)
            {
                disruptor.Halt();
            }
            await Task.FromResult(Task.CompletedTask);
        }
    }

    //public class DisruptorExecution
    //{
    //    private int ringBufferSize;
    //    private Disruptor<TickerEvent>? disruptor;
    //    public bool IsStarted { get { return disruptor != null && disruptor.HasStarted; } }
    //    public DisruptorExecution(int ringBufferSize)
    //    {
    //        // TODO: Validate ring buffer size is a power of 2 and perhaps log a warning and then fix it (round up)
    //        this.ringBufferSize = ringBufferSize;
    //    }

    //    public async Task StartDisruptor()
    //    {
    //        await Task.Run(() =>
    //        {
    //            disruptor = new Disruptor<TickerEvent>(() => new TickerEvent(), ringBufferSize);

    //            //disruptor.HandleEventsWith(new ReplicationConsumer()).Then(new JournalConsumer()).Then(new TickerEventHandler());

    //            //disruptor.HandleEventsWith(new ReplicationConsumer(), new JournalConsumer()).Then(new TickerEventHandler());

    //            disruptor.HandleEventsWith(new ReplicationConsumer(), new JournalConsumer()).Then(new StepGridStrategy());

    //            var producer = new TickerProducer(@"C:\Users\tcott\OneDrive\Apps\gunbot\Disruptor-net\src\Disruptor.MySamples\Data\ETHUSD_1.csv");

    //            disruptor.Start();

    //            producer.ProduceEvents(disruptor.PublishTickerEvents);

    //        });
    //        //await Task.FromResult(Task.CompletedTask);
    //    }

    //    public async Task StopDisruptor()
    //    {
    //        if (disruptor != null)
    //        {
    //            disruptor.Halt();
    //        }
    //        await Task.FromResult(Task.CompletedTask);
    //    }
    //}
}
