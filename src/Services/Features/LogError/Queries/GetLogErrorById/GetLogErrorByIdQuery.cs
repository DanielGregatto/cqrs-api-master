using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.LogError.Queries.GetLogErrorById
{
    public class GetLogErrorByIdQuery : IRequest<Result<LogErrorDto>>
    {
        public Guid Id { get; set; }
    }
}
