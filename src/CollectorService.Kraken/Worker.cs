using System.Resources;
using System.Security.Cryptography;
using System.Text;
using CollectorService.Kraken.Models;
using CollectorService.Kraken.Settings;
using CryptoExchange.Net.Authentication;
using Kraken.Net.Clients;
using Kraken.Net.Objects;
using Kraken.Net.Objects.Models;
using MediatR;
using ProtectData;
using AutoMapper;

namespace CollectorService.Kraken;

public class Worker : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly ILogger<Worker> _logger;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private List<Models.KrakenStreamTick> krakenStreamTicks;

    public Worker(IMediator mediator, AppSettings appSettings, ILogger<Worker> logger, IMapper mapper)
    {
        _mediator = mediator;
        _appSettings = appSettings;
        _mapper = mapper;
        _logger = logger;
        krakenStreamTicks = new List<Models.KrakenStreamTick>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO: Break out the resource data into a service and can create the Kraken options
            // TODO: Add metrics of some form and/or health monitor so we can know if it is running or not and notify when not or erroring
            // TODO: Write the current list of trades to Kafka and then add the ability to see where we are and add as new trades happen
            // TODO: Make the key read only and create a write key for the service actually in charge of doing the trades
            var keys = SecureDataService.GetKrakenKeys(GetEmbeddedResource);
            var krakenOptions = new KrakenClientOptions()
            {
                ApiCredentials = new ApiCredentials(keys.Item1, keys.Item2),
                RequestTimeout = TimeSpan.FromSeconds(10)
            };
            var krakenSocketOptions = new KrakenSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials(keys.Item1, keys.Item2),
                AutoReconnect = true,
                ReconnectInterval = TimeSpan.FromSeconds(10)
            };
            var krakenSocket = new KrakenSocketClient(krakenSocketOptions);
            foreach (string pair in _appSettings.TradingPairs)
            {
                var subscribeResult = await krakenSocket.SpotStreams.SubscribeToTickerUpdatesAsync(pair, data =>
                {
                    //var dataTick = new KrakenTickData(pair, data.Data, data.Timestamp, data.Topic);
                    //var dataTick = new KrakenTickData { TradingPair = pair, TickData = data.Data, Timestamp = data.Timestamp, Topic = data.Topic };
                    var krakenStreamTick = _mapper.Map<Models.KrakenStreamTick>(data.Data);
                    //_mapper.Map(data.Data, krakenStreamTick, typeof(KrakenStreamTick), typeof(CollectorService.Kraken.Models.KrakenStreamTick));

                    // Handle ticker data
                    _mediator.Publish(krakenStreamTick, stoppingToken);
                    krakenStreamTicks.Add(krakenStreamTick);
                });
                if (!subscribeResult.Success && subscribeResult.Error != null)
                {
                    // TODO: Log the failure
                    this._logger.LogError($"Subscribe to Ticker failed: {subscribeResult.Error.ToString()}");
                }
            }
            // Subscribe to order updates
            //var socketToken = await krakenClient.SpotApi.Account.GetWebsocketTokenAsync();
            //if (!socketToken.Success)
            //{
            //    // Handle error
            //    return;
            //}
            //var symbolData = await krakenSocket.SpotStreams.SubscribeToOrderUpdatesAsync(socketToken.Data.Token, data =>
            //{
            //    // Handle update
            //});

            //var krakenService = new KrakenService(krakenOptions);
            //var tradeHist = await krakenService.GetTradeHistoryAsync();
            //foreach (var trade in tradeHist.Keys)
            //{
            //    // TODO: Write any new history to Kafka
            //    var values = tradeHist[trade];
            //}


            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
        
    }
    private object GetEmbeddedResource(string resourceName)
    {
        if (!String.IsNullOrWhiteSpace(resourceName))
        {
            object? resource = Properties.Resources.ResourceManager.GetObject(resourceName);
            if (resource != null)
            {
                return resource;
            }
        }
        return new object();
    }
}
