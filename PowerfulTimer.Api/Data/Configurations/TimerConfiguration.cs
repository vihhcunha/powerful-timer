using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PowerfulTimer.Api.Data.Configurations;

public class TimerConfiguration : IEntityTypeConfiguration<Entities.Timer>
{
    public void Configure(EntityTypeBuilder<Entities.Timer> builder)
    {
        builder.HasKey(x => x.TimerId);
    }
}
