using Auth.BusinessLayer.Models;
using AutoMapper;
using Marvelous.Contracts.ExchangeModels;

namespace Auth.API.Configuration;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<LeadAuthExchangeModel, LeadAuthModel>();
        CreateMap<LeadFullExchangeModel, LeadAuthModel>()
            .ForMember(lam => lam.HashPassword, opt => opt.MapFrom(lfem => lfem.Password));
    }
}