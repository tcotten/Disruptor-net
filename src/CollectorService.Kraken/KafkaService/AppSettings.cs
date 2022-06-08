using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaService.Settings;

public class AppSettings
{
    public AppSettings()
    {
        BootstrapServers = "192.168.1.60:9092";
    }
    public string BootstrapServers { get; set; }
    public string? GroupId { get; set; }
}
