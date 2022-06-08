using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorService.Kraken.Settings;

public class AppSettings
{
    public AppSettings()
    {
        TradingPairs = new List<string>();
    }
    public List<string> TradingPairs { get; set; }

}
