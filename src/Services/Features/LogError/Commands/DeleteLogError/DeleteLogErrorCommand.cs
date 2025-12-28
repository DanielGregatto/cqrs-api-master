using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using MediatR;
using System;

namespace Services.Features.LogError.Commands.DeleteLogError
{
    public class DeleteLogErrorCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
