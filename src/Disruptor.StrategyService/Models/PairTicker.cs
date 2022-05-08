using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Models
{
    public record class PairTicker : GraphModelBaseRecord
    {
        public string? PairName { get; set; }
        public double bid { get; set; } = default(double)!;
        public double ask { get; set; } = default(double)!;
    }
}
