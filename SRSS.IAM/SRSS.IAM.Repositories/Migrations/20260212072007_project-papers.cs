using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class projectpapers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "papers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_papers_project_id",
                table: "papers",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_papers_systematic_review_projects_project_id",
                table: "papers",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papers_systematic_review_projects_project_id",
                table: "papers");

            migrationBuilder.DropIndex(
                name: "IX_papers_project_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "papers");
        }
    }
}
