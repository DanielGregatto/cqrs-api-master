using System;

namespace Domain.DTO.Responses
{
    public class LogErrorDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Code { get; set; }
        public string Record { get; set; }
        public DateTime CreatedAt { get; set; }

        public static LogErrorDto FromEntity(LogError logError)
        {
            if (logError == null) return null;

            return new LogErrorDto
            {
                Id = logError.Id,
                UserId = logError.UserId,
                Code = logError.Code,
                Record = logError.Record,
                CreatedAt = logError.CreatedAt
            };
        }
    }
}
