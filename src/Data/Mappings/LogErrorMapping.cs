using Data.Extensions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Mappings
{
    public class LogErrorMapping : EntityTypeConfiguration<LogError>
    {
        public LogErrorMapping(string schema) : base(schema)
        {
        }

        public override void Map(EntityTypeBuilder<LogError> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Record).IsRequired();

            builder.ToTable(Schema + "LogError");
        }
    }
}