﻿using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaService;
// Taken from: https://github.com/confluentinc/confluent-kafka-dotnet/tree/master/examples/Web

/// <summary>
///     Wraps a Confluent.Kafka.IProducer instance, and allows for basic
///     configuration of this via IConfiguration.
///    
///     KafkaClientHandle does not provide any way for messages to be produced
///     directly. Instead, it is a dependency of KafkaDependentProducer. You
///     can create more than one instance of KafkaDependentProducer (with
///     possibly differing K and V generic types) that leverages the same
///     underlying producer instance exposed by the Handle property of this
///     class. This is more efficient than creating separate
///     Confluent.Kafka.IProducer instances for each Message type you wish to
///     produce.
/// </summary>
public class KafkaClientHandle : IDisposable
{
    IProducer<byte[], byte[]> kafkaProducer;

    public KafkaClientHandle(IConfiguration config)
    {
        var conf = new ProducerConfig();
        config.GetSection("Kafka:ProducerSettings").Bind(conf);
        this.kafkaProducer = new ProducerBuilder<byte[], byte[]>(conf).Build();
    }

    public Handle Handle { get => this.kafkaProducer.Handle; }

    public void Dispose()
    {
        // Block until all outstanding produce requests have completed (with or
        // without error).
        kafkaProducer.Flush();
        kafkaProducer.Dispose();
    }
}
