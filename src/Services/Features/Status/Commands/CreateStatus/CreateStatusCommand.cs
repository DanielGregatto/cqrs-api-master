using Domain.DTO.Infrastructure.CQRS;using Domain.Enums;
using Domain.DTO.Responses;
using MediatR;

namespace Services.Features.Status.Commands.CreateStatus
{
    public class CreateStatusCommand : IRequest<Result<StatusDto>>
    {
        public string Escopo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Bloquear { get; set; }
        public bool Ativo { get; set; }
    }
}
