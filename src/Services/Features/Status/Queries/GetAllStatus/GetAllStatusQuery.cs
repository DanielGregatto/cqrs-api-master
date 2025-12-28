using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Responses;
using MediatR;
using System.Collections.Generic;

namespace Services.Features.Status.Queries.GetAllStatus
{
    public class GetAllStatusQuery : IRequest<Result<IEnumerable<StatusDto>>>
    {
    }
}
