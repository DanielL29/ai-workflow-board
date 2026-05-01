using AiBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiBoard.Infrastructure.Persistence.Configurations;

public sealed class BoardMemoryChunkConfiguration : IEntityTypeConfiguration<BoardMemoryChunk>
{
    public void Configure(EntityTypeBuilder<BoardMemoryChunk> builder)
    {
        builder.ToTable("board_memory_chunks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.Embedding).HasColumnType("vector(384)").IsRequired();
        builder.HasIndex(x => new { x.BoardId, x.DocumentId, x.Sequence });
    }
}
