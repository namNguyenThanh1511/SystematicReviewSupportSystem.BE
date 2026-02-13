using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentPhaseToReviewProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_review_process_project_type_unique",
                table: "review_processes");

            migrationBuilder.RenameColumn(
                name: "process_type",
                table: "review_processes",
                newName: "current_phase");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "systematic_review_projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_identification_processes_review_process_id",
                table: "identification_processes",
                column: "review_process_id");

            migrationBuilder.AddForeignKey(
                name: "FK_identification_processes_review_processes_review_process_id",
                table: "identification_processes",
                column: "review_process_id",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_identification_processes_review_processes_review_process_id",
                table: "identification_processes");

            migrationBuilder.DropIndex(
                name: "IX_identification_processes_review_process_id",
                table: "identification_processes");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "systematic_review_projects");

            migrationBuilder.RenameColumn(
                name: "current_phase",
                table: "review_processes",
                newName: "process_type");

            migrationBuilder.CreateIndex(
                name: "idx_review_process_project_type_unique",
                table: "review_processes",
                columns: new[] { "project_id", "process_type" },
                unique: true);
        }
    }
}
