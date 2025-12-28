using Data.Extensions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Mappings
{
    public class StatusMapping : EntityTypeConfiguration<Status>
    {
        public StatusMapping(string schema) : base(schema)
        {
        }

        public override void Map(EntityTypeBuilder<Status> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(Schema + "Status");
        }
    }
}