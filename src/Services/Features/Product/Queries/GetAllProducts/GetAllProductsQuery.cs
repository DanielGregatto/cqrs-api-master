using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System.Collections.Generic;

namespace Services.Features.Product.Queries.GetAllProducts
{
    public class GetAllProductsQuery : IRequest<Result<IEnumerable<ProductDto>>>
    {
    }
}
