using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.Product.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<Result<ProductDto>>
    {
        public Guid Id { get; set; }

        public GetProductByIdQuery()
        {
        }

        public GetProductByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
