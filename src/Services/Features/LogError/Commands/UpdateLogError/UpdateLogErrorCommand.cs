using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.LogError.Commands.UpdateLogError
{
    public class UpdateLogErrorCommand : IRequest<Result<LogErrorDto>>
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Code { get; set; }
        public string Record { get; set; }
    }
}
