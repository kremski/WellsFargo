using AutoMapper;
using WellsFargo.DAL.Model;
using WellsFargo.Helpers.OmsOutputModels;

namespace WellsFargo.Automapper
{
    public class OmsOutputProfile : Profile
    {
        public OmsOutputProfile()
        {
            CreateMap<Transaction, OmsOutputBaseModel>()
                .IncludeAllDerived()
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
                .ForPath(dest => dest.TransactionType, opt => opt.MapFrom(src => src.TransactionType.TransactionTypeCode))
                .ForPath(dest => dest.PortfolioCode, opt => opt.MapFrom(src => src.Portfolio.PortfolioCode))
                .ForMember(dest => dest.Nominal, opt => opt.MapFrom(src => src.Nominal));

            CreateMap<Transaction, OmsOutputAaaModel>()
                .ForPath(dest => dest.Isin, opt => opt.MapFrom(src => src.Security.ISIN));

            CreateMap<Transaction, OmsOutputBbbModel>()
                .ForPath(dest => dest.Cusip, opt => opt.MapFrom(src => src.Security.Cusip));

            CreateMap<Transaction, OmsOutputCccModel>()
                .ForPath(dest => dest.Ticker, opt => opt.MapFrom(src => src.Security.Ticker));
        }
    }
}