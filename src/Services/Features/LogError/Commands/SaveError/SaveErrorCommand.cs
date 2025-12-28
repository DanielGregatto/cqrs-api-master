using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.LogError.Commands.SaveError
{
    public class SaveErrorCommand : IRequest<Result<LogErrorDto>>
    {
        public string ErrorDescription { get; set; }
        public string Code { get; set; }
        public Guid? UserId { get; set; }
    }
}
