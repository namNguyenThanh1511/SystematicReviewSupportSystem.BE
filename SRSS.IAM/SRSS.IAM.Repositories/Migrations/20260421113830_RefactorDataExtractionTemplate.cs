using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDataExtractionTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_extraction_template_systematic_review_projects_ProjectId",
                table: "extraction_template");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "extraction_template",
                newName: "DataExtractionProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_extraction_template_ProjectId",
                table: "extraction_template",
                newName: "IX_extraction_template_DataExtractionProcessId");

            migrationBuilder.AddColumn<bool>(
                name: "is_picoc",
                table: "extraction_section",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "linked_research_question_id",
                table: "extraction_section",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_extraction_section_linked_research_question_id",
                table: "extraction_section",
                column: "linked_research_question_id");

            migrationBuilder.AddForeignKey(
                name: "FK_extraction_section_research_question_linked_research_questi~",
                table: "extraction_section",
                column: "linked_research_question_id",
                principalTable: "research_question",
                principalColumn: "research_question_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_extraction_template_data_extraction_process_DataExtractionP~",
                table: "extraction_template",
                column: "DataExtractionProcessId",
                principalTable: "data_extraction_process",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_extraction_section_research_question_linked_research_questi~",
                table: "extraction_section");

            migrationBuilder.DropForeignKey(
                name: "FK_extraction_template_data_extraction_process_DataExtractionP~",
                table: "extraction_template");

            migrationBuilder.DropIndex(
                name: "IX_extraction_section_linked_research_question_id",
                table: "extraction_section");

            migrationBuilder.DropColumn(
                name: "is_picoc",
                table: "extraction_section");

            migrationBuilder.DropColumn(
                name: "linked_research_question_id",
                table: "extraction_section");

            migrationBuilder.RenameColumn(
                name: "DataExtractionProcessId",
                table: "extraction_template",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_extraction_template_DataExtractionProcessId",
                table: "extraction_template",
                newName: "IX_extraction_template_ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_extraction_template_systematic_review_projects_ProjectId",
                table: "extraction_template",
                column: "ProjectId",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
