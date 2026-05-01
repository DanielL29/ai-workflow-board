using AiBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiBoard.Infrastructure.Persistence.Configurations;

public sealed class BoardMemoryDocumentConfiguration : IEntityTypeConfiguration<BoardMemoryDocument>
{
    public void Configure(EntityTypeBuilder<BoardMemoryDocument> builder)
    {
        builder.ToTable("board_memory_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(64000).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.HasIndex(x => new { x.BoardId, x.SourceNodeId, x.SourceType });
        builder.Metadata.FindNavigation(nameof(BoardMemoryDocument.Chunks))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.Chunks).WithOne().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
    }
}
