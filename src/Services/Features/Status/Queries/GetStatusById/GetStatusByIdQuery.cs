using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.Status.Queries.GetStatusById
{
    public class GetStatusByIdQuery : IRequest<Result<StatusDto>>
    {
        public Guid Id { get; set; }

        public GetStatusByIdQuery()
        {
        }

        public GetStatusByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
