using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Infrastructure.API;
using Domain.DTO.Responses;
using MediatR;
using System;
using System.Linq.Expressions;

namespace Services.Features.Product.Queries.SearchProductsPaginated
{
    public class SearchProductsPaginatedQuery : IRequest<Result<PaginatedResponseDto<ProductDto>>>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Expression<Func<Domain.Product, bool>> Predicate { get; set; }
        public string OrderByProperty { get; set; }
        public bool OrderByDescending { get; set; } = false;
    }
}
