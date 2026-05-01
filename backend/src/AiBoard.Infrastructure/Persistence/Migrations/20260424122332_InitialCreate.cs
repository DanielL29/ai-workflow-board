using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "boards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "generation_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Prompt = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResultPayload = table.Column<string>(type: "character varying(32000)", maxLength: 32000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generation_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "board_edges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_board_edges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_board_edges_boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "board_nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Content = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OutputContent = table.Column<string>(type: "character varying(24000)", maxLength: 24000, nullable: true),
                    position_x = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    position_y = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_board_nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_board_nodes_boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_board_edges_BoardId",
                table: "board_edges",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_board_nodes_BoardId",
                table: "board_nodes",
                column: "BoardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "board_edges");

            migrationBuilder.DropTable(
                name: "board_nodes");

            migrationBuilder.DropTable(
                name: "generation_jobs");

            migrationBuilder.DropTable(
                name: "boards");
        }
    }
}
