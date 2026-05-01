using AiBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiBoard.Infrastructure.Persistence.Configurations;

public sealed class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("boards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Metadata.FindNavigation(nameof(Board.Nodes))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Board.Edges))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Board.MemoryDocuments))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.Nodes).WithOne().HasForeignKey(x => x.BoardId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Edges).WithOne().HasForeignKey(x => x.BoardId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.MemoryDocuments).WithOne().HasForeignKey(x => x.BoardId).OnDelete(DeleteBehavior.Cascade);
    }
}
