using AiBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiBoard.Infrastructure.Persistence.Configurations;

public sealed class GenerationJobConfiguration : IEntityTypeConfiguration<GenerationJob>
{
    public void Configure(EntityTypeBuilder<GenerationJob> builder)
    {
        builder.ToTable("generation_jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Prompt).HasMaxLength(16000).IsRequired();
        builder.Property(x => x.ResultPayload).HasMaxLength(32000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
