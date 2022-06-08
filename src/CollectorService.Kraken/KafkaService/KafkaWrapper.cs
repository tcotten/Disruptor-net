using Confluent.Kafka;
using Confluent.Kafka.Admin;
using KafkaService.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaService;

public class KafkaWrapper
{
    private AppSettings appSettings;
    public KafkaWrapper(AppSettings appSettings)
    {
        this.appSettings = appSettings;
        if (String.IsNullOrWhiteSpace(appSettings.GroupId))
        {
            throw new ArgumentNullException("GroupId not specified which won't allow correct topic tracking.");
        }
    }
    public async Task CreateTopicAsync(string topicName, int numPartitions = 1, short replicationFactor = 1)
    {
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = appSettings.BootstrapServers }).Build())
        {
            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[] {
                        new TopicSpecification { Name = topicName, ReplicationFactor = replicationFactor, NumPartitions = numPartitions } });
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }
        }
    }
}
