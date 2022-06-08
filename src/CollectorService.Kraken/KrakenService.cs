using CryptoExchange.Net.Authentication;
using Kraken.Net.Clients;
using Kraken.Net.Objects;
using Kraken.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorService.Kraken
{
    public class KrakenService
    {
        private KrakenClient krakenClient;
        public KrakenService(KrakenClientOptions options)
        {
            krakenClient = new KrakenClient(options);
        }
        public async Task<Dictionary<string, KrakenUserTrade>> GetTradeHistoryAsync()
        {
            var tradeHistoryData = await krakenClient.SpotApi.Trading.GetUserTradesAsync();
            if (null != tradeHistoryData && tradeHistoryData.Success)
            {
                return tradeHistoryData.Data.Trades;
            }
            // TODO: Need to log this error and collect metrics since we know this can happen depending on the message
            return new Dictionary<string, KrakenUserTrade>();
        }
    }
}
