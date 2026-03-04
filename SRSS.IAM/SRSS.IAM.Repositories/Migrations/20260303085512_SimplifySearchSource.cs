using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SimplifySearchSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bibliographic_database");

            migrationBuilder.DropTable(
                name: "conference_proceeding");

            migrationBuilder.DropTable(
                name: "digital_library");

            migrationBuilder.DropTable(
                name: "journal");

            migrationBuilder.DropTable(
                name: "search_string_term");

            migrationBuilder.DropTable(
                name: "search_string");

            migrationBuilder.DropTable(
                name: "search_term");

            migrationBuilder.DropTable(
                name: "search_strategy");

            migrationBuilder.DropColumn(
                name: "source_type",
                table: "search_source");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "review_protocol",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "review_protocol",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "review_protocol",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "planned_date",
                table: "project_timetable",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "review_protocol");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "review_protocol");

            migrationBuilder.AddColumn<string>(
                name: "source_type",
                table: "search_source",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "review_protocol",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "planned_date",
                table: "project_timetable",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "bibliographic_database",
                columns: table => new
                {
                    database_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bibliographic_database", x => x.database_id);
                    table.ForeignKey(
                        name: "FK_bibliographic_database_search_source_source_id",
                        column: x => x.source_id,
                        principalTable: "search_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conference_proceeding",
                columns: table => new
                {
                    conference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conference_proceeding", x => x.conference_id);
                    table.ForeignKey(
                        name: "FK_conference_proceeding_search_source_source_id",
                        column: x => x.source_id,
                        principalTable: "search_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "digital_library",
                columns: table => new
                {
                    library_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_digital_library", x => x.library_id);
                    table.ForeignKey(
                        name: "FK_digital_library_search_source_source_id",
                        column: x => x.source_id,
                        principalTable: "search_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal",
                columns: table => new
                {
                    journal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal", x => x.journal_id);
                    table.ForeignKey(
                        name: "FK_journal_search_source_source_id",
                        column: x => x.source_id,
                        principalTable: "search_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_strategy",
                columns: table => new
                {
                    strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_strategy", x => x.strategy_id);
                    table.ForeignKey(
                        name: "FK_search_strategy_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_term",
                columns: table => new
                {
                    term_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    keyword = table.Column<string>(type: "text", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_term", x => x.term_id);
                });

            migrationBuilder.CreateTable(
                name: "search_string",
                columns: table => new
                {
                    search_string_id = table.Column<Guid>(type: "uuid", nullable: false),
                    strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expression = table.Column<string>(type: "text", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_string", x => x.search_string_id);
                    table.ForeignKey(
                        name: "FK_search_string_search_strategy_strategy_id",
                        column: x => x.strategy_id,
                        principalTable: "search_strategy",
                        principalColumn: "strategy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_string_term",
                columns: table => new
                {
                    search_string_id = table.Column<Guid>(type: "uuid", nullable: false),
                    term_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_string_term", x => new { x.search_string_id, x.term_id });
                    table.ForeignKey(
                        name: "FK_search_string_term_search_string_search_string_id",
                        column: x => x.search_string_id,
                        principalTable: "search_string",
                        principalColumn: "search_string_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_search_string_term_search_term_term_id",
                        column: x => x.term_id,
                        principalTable: "search_term",
                        principalColumn: "term_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bibliographic_database_source_id",
                table: "bibliographic_database",
                column: "source_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conference_proceeding_source_id",
                table: "conference_proceeding",
                column: "source_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_digital_library_source_id",
                table: "digital_library",
                column: "source_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journal_source_id",
                table: "journal",
                column: "source_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_strategy_protocol_id",
                table: "search_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_string_strategy_id",
                table: "search_string",
                column: "strategy_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_string_term_term_id",
                table: "search_string_term",
                column: "term_id");
        }
    }
}
