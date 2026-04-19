using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDisseminationAndRelocateTimetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"project_timetable\";");

            migrationBuilder.DropForeignKey(
                name: "FK_project_timetable_review_protocol_protocol_id",
                table: "project_timetable");

            migrationBuilder.DropTable(
                name: "data_item_definition");

            migrationBuilder.DropTable(
                name: "dissemination_strategy");

            migrationBuilder.DropTable(
                name: "data_extraction_form");

            migrationBuilder.DropTable(
                name: "data_extraction_strategy");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "project_timetable",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_project_timetable_protocol_id",
                table: "project_timetable",
                newName: "IX_project_timetable_project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_project_timetable_systematic_review_projects_project_id",
                table: "project_timetable",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_timetable_systematic_review_projects_project_id",
                table: "project_timetable");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "project_timetable",
                newName: "protocol_id");

            migrationBuilder.RenameIndex(
                name: "IX_project_timetable_project_id",
                table: "project_timetable",
                newName: "IX_project_timetable_protocol_id");

            migrationBuilder.CreateTable(
                name: "data_extraction_strategy",
                columns: table => new
                {
                    extraction_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_extraction_strategy", x => x.extraction_strategy_id);
                    table.ForeignKey(
                        name: "FK_data_extraction_strategy_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dissemination_strategy",
                columns: table => new
                {
                    dissemination_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dissemination_strategy", x => x.dissemination_id);
                    table.ForeignKey(
                        name: "FK_dissemination_strategy_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_extraction_form",
                columns: table => new
                {
                    form_id = table.Column<Guid>(type: "uuid", nullable: false),
                    extraction_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_extraction_form", x => x.form_id);
                    table.ForeignKey(
                        name: "FK_data_extraction_form_data_extraction_strategy_extraction_st~",
                        column: x => x.extraction_strategy_id,
                        principalTable: "data_extraction_strategy",
                        principalColumn: "extraction_strategy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_item_definition",
                columns: table => new
                {
                    data_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    form_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data_type = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_item_definition", x => x.data_item_id);
                    table.ForeignKey(
                        name: "FK_data_item_definition_data_extraction_form_form_id",
                        column: x => x.form_id,
                        principalTable: "data_extraction_form",
                        principalColumn: "form_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_extraction_form_extraction_strategy_id",
                table: "data_extraction_form",
                column: "extraction_strategy_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_extraction_strategy_protocol_id",
                table: "data_extraction_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_item_definition_form_id",
                table: "data_item_definition",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "IX_dissemination_strategy_protocol_id",
                table: "dissemination_strategy",
                column: "protocol_id");

            migrationBuilder.AddForeignKey(
                name: "FK_project_timetable_review_protocol_protocol_id",
                table: "project_timetable",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
