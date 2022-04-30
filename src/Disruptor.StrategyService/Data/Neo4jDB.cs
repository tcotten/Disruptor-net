using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient;

namespace Disruptor.StrategyService.Data;

public class Neo4jDB
{
    public GraphClient NeoClient { get; private set; }
    public Neo4jDB()
    {
        NeoClient = new GraphClient(new Uri("http://172.2.96.1:7474"));
        //var config = new NeoServerConfiguration();

        NeoClient.ConnectAsync().GetAwaiter().GetResult();
    }

}
