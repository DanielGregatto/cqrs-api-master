using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.Account.Commands.UpdatePersonalInfo
{
    public class UpdatePersonalInfoCommand : IRequest<Result<ProfileDto>>
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CPF_CNPJ { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
