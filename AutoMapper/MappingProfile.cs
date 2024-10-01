using AutoMapper;
using TradingApp.Models;

namespace TradingApp.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Invoice, Invoice>();
            CreateMap<InvoiceLine, InvoiceLine>();
        }
    }

}
