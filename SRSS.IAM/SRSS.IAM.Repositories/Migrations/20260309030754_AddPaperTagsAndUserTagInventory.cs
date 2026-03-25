using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPaperTagsAndUserTagInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "study_selection_procedure",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "study_selection_procedure",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "review_protocol",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "quality_criterion",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "quality_criterion",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "quality_checklist",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "quality_checklist",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "quality_assessment_strategy",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "quality_assessment_strategy",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "project_timetable",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "project_timetable",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "project_members",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "project_members",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "population",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "population",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "picoc_element",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "picoc_element",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "outcome",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "outcome",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "intervention",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "intervention",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "dissemination_strategy",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "dissemination_strategy",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "data_synthesis_strategy",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "data_synthesis_strategy",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "data_item_definition",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "data_item_definition",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "data_extraction_strategy",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "data_extraction_strategy",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "data_extraction_form",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "data_extraction_form",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "context",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "context",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "comparison",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "comparison",
                newName: "created_at");

            migrationBuilder.CreateTable(
                name: "paper_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phase = table.Column<string>(type: "text", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_paper_tags_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_paper_tags_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_tag_inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phase = table.Column<string>(type: "text", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tag_inventory", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_tag_inventory_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_paper_tags_paper_id",
                table: "paper_tags",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "ix_paper_tags_paper_user_phase_label",
                table: "paper_tags",
                columns: new[] { "paper_id", "user_id", "phase", "label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_paper_tags_user_phase",
                table: "paper_tags",
                columns: new[] { "user_id", "phase" });

            migrationBuilder.CreateIndex(
                name: "ix_user_tag_inventory_user_phase",
                table: "user_tag_inventory",
                columns: new[] { "user_id", "phase" });

            migrationBuilder.CreateIndex(
                name: "ix_user_tag_inventory_user_phase_label",
                table: "user_tag_inventory",
                columns: new[] { "user_id", "phase", "label" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paper_tags");

            migrationBuilder.DropTable(
                name: "user_tag_inventory");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "study_selection_procedure",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "study_selection_procedure",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "review_protocol",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "quality_criterion",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "quality_criterion",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "quality_checklist",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "quality_checklist",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "quality_assessment_strategy",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "quality_assessment_strategy",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "project_timetable",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "project_timetable",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "project_members",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "project_members",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "population",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "population",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "picoc_element",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "picoc_element",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "outcome",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "outcome",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "intervention",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "intervention",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "dissemination_strategy",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "dissemination_strategy",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "data_synthesis_strategy",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "data_synthesis_strategy",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "data_item_definition",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "data_item_definition",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "data_extraction_strategy",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "data_extraction_strategy",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "data_extraction_form",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "data_extraction_form",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "context",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "context",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "comparison",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "comparison",
                newName: "CreatedAt");
        }
    }
}
