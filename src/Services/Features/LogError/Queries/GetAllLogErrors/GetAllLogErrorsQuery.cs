using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Responses;
using MediatR;
using System.Collections.Generic;

namespace Services.Features.LogError.Queries.GetAllLogErrors
{
    public class GetAllLogErrorsQuery : IRequest<Result<IEnumerable<LogErrorDto>>>
    {
    }
}
