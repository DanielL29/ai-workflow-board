using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace AiBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardMemoryRag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "board_memory_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(64000)", maxLength: 64000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_board_memory_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_board_memory_documents_boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "board_memory_chunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(384)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_board_memory_chunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_board_memory_chunks_board_memory_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "board_memory_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_board_memory_chunks_BoardId_DocumentId_Sequence",
                table: "board_memory_chunks",
                columns: new[] { "BoardId", "DocumentId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_board_memory_chunks_DocumentId",
                table: "board_memory_chunks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_board_memory_documents_BoardId_SourceNodeId_SourceType",
                table: "board_memory_documents",
                columns: new[] { "BoardId", "SourceNodeId", "SourceType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "board_memory_chunks");

            migrationBuilder.DropTable(
                name: "board_memory_documents");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
