using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using MediatR;
using System;

namespace Services.Features.Status.Queries.GetIdStatusInativo
{
    public class GetIdStatusInativoQuery : IRequest<Result<Guid>>
    {
    }
}
