using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using MediatR;
using System;

namespace Services.Features.Status.Queries.GetIdStatusAtivo
{
    public class GetIdStatusAtivoQuery : IRequest<Result<Guid>>
    {
    }
}
