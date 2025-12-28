using Domain.Core;
using System;

namespace Domain
{
    public class LogError : EntityBase<LogError>
    {
        public Guid? UserId { get; set; }
        public string Code { get; set; }
        public string Record { get; set; }
    }
}