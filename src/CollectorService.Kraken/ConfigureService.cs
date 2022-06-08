using CollectorService.Kraken.Models;
using CollectorService.Kraken.Settings;
using KafkaService;
using Kraken.Net.Objects.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace CollectorService.Kraken;

public static class ConfigureService
{
    public static IServiceCollection AddKrakenCollectorServices(this IServiceCollection services)
    {
        services.AddSingleton<AppSettings>(app =>
        {
           var appSettings = new AppSettings();
            appSettings.TradingPairs.AddRange(new[] { "XBT/USD", "ETH/USD" });
            return appSettings;
        });
        services.AddHealthChecks();
        services.AddAutoMapper(typeof(Profiles.MappingProfile));
        //Mapper.AssertConfigurationIsValid();
        services.AddMediatR(typeof(Worker));
        services.AddSingleton<KafkaClientHandle>();
        services.AddSingleton<KafkaDependentProducer<string, KrakenTickData>>();
        services.AddSingleton<INotificationHandler<Models.KrakenStreamTick>, KrakenStreamTickMediatrHandler>();

        return services;
    }
}
