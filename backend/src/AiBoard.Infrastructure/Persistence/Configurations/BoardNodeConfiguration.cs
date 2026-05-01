using AiBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiBoard.Infrastructure.Persistence.Configurations;

public sealed class BoardNodeConfiguration : IEntityTypeConfiguration<BoardNode>
{
    public void Configure(EntityTypeBuilder<BoardNode> builder)
    {
        builder.ToTable("board_nodes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(12000).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.OutputContent).HasMaxLength(24000);
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.OwnsOne(
            x => x.Position,
            position =>
            {
                position.Property(p => p.X).HasColumnName("position_x").HasColumnType("numeric(10,2)");
                position.Property(p => p.Y).HasColumnName("position_y").HasColumnType("numeric(10,2)");
            });
    }
}
