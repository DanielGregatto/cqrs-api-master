using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Infrastructure.API;
using Domain.DTO.Responses;
using MediatR;
using System;
using System.Linq.Expressions;

namespace Services.Features.Status.Queries.SearchStatusPaginated
{
    public class SearchStatusPaginatedQuery : IRequest<Result<PaginatedResponseDto<StatusDto>>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Expression<Func<Domain.Status, bool>> Predicate { get; set; }
        public string OrderByProperty { get; set; }
        public bool OrderByDescending { get; set; } = false;
    }
}
