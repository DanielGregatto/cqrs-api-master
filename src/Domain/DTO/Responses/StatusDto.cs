using System;

namespace Domain.DTO.Responses
{
    public class StatusDto
    {
        public Guid Id { get; set; }
        public string Escopo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Bloquear { get; set; }
        public bool Ativo { get; set; }
        public DateTime CreatedAt { get; set; }

        public static StatusDto FromEntity(Status status)
        {
            if (status == null) return null;

            return new StatusDto
            {
                Id = status.Id,
                Escopo = status.Escopo,
                Nome = status.Nome,
                Descricao = status.Descricao,
                Bloquear = status.Bloquear,
                Ativo = status.Ativo,
                CreatedAt = status.CreatedAt
            };
        }
    }
}
