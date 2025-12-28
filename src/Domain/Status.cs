using Domain.Core;

namespace Domain
{
    public class Status : EntityBase<Status>
    {
        public string Escopo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Bloquear { get; set; }
        public bool Ativo { get; set; }
    }
}