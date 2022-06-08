using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kk = Kraken.Net.Objects.Models;
using CollectorService.Kraken.Models;

namespace CollectorService.Kraken.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<kk.KrakenStreamTick, KrakenStreamTick>();
        CreateMap<kk.KrakenTickInfo, KrakenTickInfo>();
        CreateMap<kk.KrakenBestEntry, KrakenBestEntry>();
        CreateMap<kk.KrakenLastTrade, KrakenLastTrade>();
    }
     
}
