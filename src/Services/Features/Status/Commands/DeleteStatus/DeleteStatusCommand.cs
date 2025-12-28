using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using MediatR;
using System;

namespace Services.Features.Status.Commands.DeleteStatus
{
    public class DeleteStatusCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
    }
}
