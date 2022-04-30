using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.MySamples
{
    public class JournalConsumer : IValueEventHandler<TickerEvent>
    {
        public void OnEvent(ref TickerEvent data, long sequence, bool endOfBatch)
        {
            Console.WriteLine($"Journal - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}");
            if (endOfBatch) { Console.WriteLine("Journal End of Batch."); }
        }
    }
}
