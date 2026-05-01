using AiBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiBoard.Infrastructure.Persistence.Configurations;

public sealed class BoardEdgeConfiguration : IEntityTypeConfiguration<BoardEdge>
{
    public void Configure(EntityTypeBuilder<BoardEdge> builder)
    {
        builder.ToTable("board_edges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).HasMaxLength(160);
    }
}
