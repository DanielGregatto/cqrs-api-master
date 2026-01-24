using AutoMapper;
using Domain.DTO.Responses;
using Identity.Model;
using Services.Features.Account.Commands.UpdateAddress;
using Services.Features.Account.Commands.UpdatePersonalInfo;

namespace UI.API.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ProfileDto>().ReverseMap();

            CreateMap<ApplicationUser, UpdatePersonalInfoCommand>().ReverseMap();

            CreateMap<ApplicationUser, UpdateAddressCommand>().ReverseMap();
        }
    }
}