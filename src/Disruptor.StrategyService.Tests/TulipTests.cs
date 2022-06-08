using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Data.Analysis;
using Skender.Stock.Indicators;
using Confluent.Kafka;
using System.Threading;

namespace Disruptor.StrategyService.Tests;

[TestClass]
public class TulipTests
{
    //[TestMethod]
    //public async Task SMAIndicatorTest()
    //{
    //    await Task.Run(() =>
    //    {
    //        //Run SMA on close prices using a smoothing period of 4 bars.

    //        double[] close_prices = new double[] { 5, 4, 5, 4, 4, 6, 5, 4, 5, 2, 5, 5, 5, 4, 4, 3 };
    //        string expected_prices = "4.5, 4.25, 4.75, 4.75, 4.75, 5, 4, 4, 4.25, 4.25, 4.75, 4.5, 4";
    //        double[] options = new double[] { 4 };

    //        //Find output size and allocate output space.
    //        int output_length = close_prices.Length - tinet.indicators.sma.start(options);
    //        double[] output = new double[output_length];

    //        double[][] inputs = { close_prices };
    //        double[][] outputs = { output };
    //        int success = tinet.indicators.sma.run(inputs, options, outputs);

    //        string closes = string.Join(", ", close_prices);
    //        string outputStr = string.Join(", ", output);
    //        Assert.AreEqual(expected_prices, outputStr);
    //    });

    //}

    [TestMethod]
    public void DictionaryTypeTest()
    {
        var indicators = new Dictionary<string, IEnumerable<dynamic>>();
        var smaResults = new List<SmaResult>()
        {
            new SmaResult() { Date = DateTime.Now, Sma = (decimal?)4.7 },
            new SmaResult() { Date = DateTime.Now, Sma = (decimal?)7.3 },
            new SmaResult() { Date = DateTime.Now, Sma = (decimal?)11.1 }
        };
        var emaResults = new List<EmaResult>()
        {
            new EmaResult() { Date = DateTime.Now, Ema = (decimal?)5.6 },
            new EmaResult() { Date = DateTime.Now, Ema = (decimal?)8.4 },
            new EmaResult() { Date = DateTime.Now, Ema = (decimal?)12.2 }
        };
        indicators.Add("SMA5", smaResults);
        indicators.Add("EMA5", emaResults);

        var emaIndList = indicators.GetList<EmaResult>("EMA5");
        Assert.IsNotNull(emaIndList);
        Assert.AreEqual(5.6m, emaIndList[0].Ema);
        var smaIndList = indicators.GetList<SmaResult>("SMA5");
        Assert.AreEqual(11.1m, smaIndList[2].Sma);
    }

    [TestMethod]
    public async void ProduceMessageTest()
    {
        var config = new ProducerConfig { BootstrapServers = "192.168.1.60:9092" };
        using (var p = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                var dr = await p.ProduceAsync("TestTopic", new Message<Null, string> { Value = "Test Message" });
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    [TestMethod]
    public void ProduceMessage2Test()
    {
        var config = new ProducerConfig { BootstrapServers = "192.168.1.60:9092" };
        using (var p = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                var dr = p.ProduceAsync("TestTopic", new Message<Null, string> { Value = "Test Message" }).GetAwaiter().GetResult();
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    [TestMethod]
    public void SubscribeMessageTest()
    {
        var conf = new ConsumerConfig
        {
            GroupId = "test-consumer-group",
            BootstrapServers = "localhost:9092",
            // Note: The AutoOffsetReset property determines the start offset in the event
            // there are not yet any committed offsets for the consumer group for the
            // topic/partitions of interest. By default, offsets are committed
            // automatically, so in this example, consumption will only start from the
            // earliest message in the topic 'my-topic' the first time you run the program.
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
        {
            c.Subscribe("TestTopic");

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(cts.Token);
                        Console.WriteLine($"Consumed message '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'.");
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();
            }
        }
    }

    [TestMethod]
    public void PubSubIntegrationTest()
    {
        new Thread(() => FakeConsumer()) { IsBackground = true }.Start();

        Thread.Sleep(5000);

        var config = new ProducerConfig { BootstrapServers = "192.168.1.60:9092" };
        using (var p = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                var dr = p.ProduceAsync("TestTopic", new Message<Null, string> { Value = "Test Message" }).GetAwaiter().GetResult();
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        Thread.Sleep(10000);
        
    }
    private void FakeConsumer()
    {
        var config = new ConsumerConfig
        {
            GroupId = "test-consumer-group",
            BootstrapServers = "localhost:9092",
            // Note: The AutoOffsetReset property determines the start offset in the event
            // there are not yet any committed offsets for the consumer group for the
            // topic/partitions of interest. By default, offsets are committed
            // automatically, so in this example, consumption will only start from the
            // earliest message in the topic 'my-topic' the first time you run the program.
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        using (var c = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            c.Subscribe(new [] { "TestTopic" });

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(cts.Token);

                        Console.WriteLine($"Consumed message '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'.");
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occurred: {e.Error.Reason}");

                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();
            }
        }
    }
}

public static class IndicatorResultExtensions
{
    public static List<T> GetList<T>(this Dictionary<string, IEnumerable<dynamic>> dictList, string indName)
    {
        if (dictList != null)
        {
            var kvp = dictList[indName];
            return (List<T>)kvp;
        }
        return new List<T>();
    }
    public static T Get<T>(this Tuple<string, Type, IEnumerable<dynamic>> tuple)
    {
        return (T)tuple.Item3;
    }
}
