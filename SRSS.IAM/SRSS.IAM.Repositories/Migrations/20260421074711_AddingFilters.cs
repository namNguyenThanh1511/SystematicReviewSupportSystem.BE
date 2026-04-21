using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddingFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "filter_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    search_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    keyword = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    year_from = table.Column<int>(type: "integer", nullable: true),
                    year_to = table.Column<int>(type: "integer", nullable: true),
                    search_source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    import_batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    doi_state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "all"),
                    full_text_state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "all"),
                    only_unused = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    recently_imported = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filter_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_filter_settings_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_process_filter_settings",
                columns: table => new
                {
                    review_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filter_setting_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_process_filter_settings", x => new { x.review_process_id, x.filter_setting_id });
                    table.ForeignKey(
                        name: "FK_review_process_filter_settings_filter_settings_filter_setti~",
                        column: x => x.filter_setting_id,
                        principalTable: "filter_settings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_review_process_filter_settings_review_processes_review_proc~",
                        column: x => x.review_process_id,
                        principalTable: "review_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_filter_setting_project_id",
                table: "filter_settings",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_filter_setting_project_name",
                table: "filter_settings",
                columns: new[] { "project_id", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_review_process_filter_settings_filter_setting_id",
                table: "review_process_filter_settings",
                column: "filter_setting_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "review_process_filter_settings");

            migrationBuilder.DropTable(
                name: "filter_settings");
        }
    }
}
