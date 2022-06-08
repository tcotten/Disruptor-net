using Confluent.Kafka;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaService;

public class KafkaNotificationHandler<T> : INotificationHandler<T> where T : INotification
{
    //private readonly ILogger _logger;
    private readonly KafkaDependentProducer<string, T> producer;
    //public string Topic { get; init; }
    /// <summary>
    /// Topic Key sent to the producer with the instance <typeparamref name="T"/>
    /// </summary>
    public string? TopicKey { get; set; }
    public KafkaNotificationHandler(KafkaDependentProducer<string, T> producer)
    {
        //Topic = topic;
        //_logger = logger;
        this.producer = producer;
    }
    
    public Task Handle(T notification, CancellationToken cancellationToken)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));
#pragma warning disable CS8601 // Possible null reference assignment.
        var msg = new Message<string, T>() { Key = TopicKey, Value = notification };
#pragma warning restore CS8601 // Possible null reference assignment.
        //producer.Produce(Topic, msg, deliveryReportHandler);
        return Task.CompletedTask;
    }
    private void deliveryReportHandler(DeliveryReport<string, T> deliveryReport)
    {
        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            //_logger.Log(LogLevel.Warning, $"Failed to log notification to Kafka for key: {deliveryReport.Message.Key} Type: {nameof(T)}");
        }
    }
}
