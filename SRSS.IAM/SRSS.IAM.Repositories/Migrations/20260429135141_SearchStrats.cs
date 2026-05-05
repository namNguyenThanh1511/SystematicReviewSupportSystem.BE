using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SearchStrats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_strategy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    query = table.Column<string>(type: "text", nullable: false),
                    fields = table.Column<string[]>(type: "text[]", nullable: false),
                    date_searched = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    filters_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_strategy", x => x.id);
                    table.ForeignKey(
                        name: "FK_search_strategy_search_source_search_source_id",
                        column: x => x.search_source_id,
                        principalTable: "search_source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_search_strategy_search_source_id",
                table: "search_strategy",
                column: "search_source_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_strategy");
        }
    }
}
