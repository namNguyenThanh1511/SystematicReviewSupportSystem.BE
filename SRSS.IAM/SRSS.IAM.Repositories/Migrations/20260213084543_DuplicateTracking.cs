using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DuplicateTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "duplicate_of_id",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_duplicate",
                table: "papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_papers_duplicate_of_id",
                table: "papers",
                column: "duplicate_of_id");

            migrationBuilder.AddForeignKey(
                name: "FK_papers_papers_duplicate_of_id",
                table: "papers",
                column: "duplicate_of_id",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papers_papers_duplicate_of_id",
                table: "papers");

            migrationBuilder.DropIndex(
                name: "IX_papers_duplicate_of_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "duplicate_of_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "is_duplicate",
                table: "papers");
        }
    }
}
