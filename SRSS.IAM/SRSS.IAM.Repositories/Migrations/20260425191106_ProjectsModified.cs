using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ProjectsModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "systematic_review_projects",
                newName: "UpdatedByUserId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActualEndDate",
                table: "systematic_review_projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActualStartDate",
                table: "systematic_review_projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "systematic_review_projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "systematic_review_projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "systematic_review_projects");

            migrationBuilder.DropColumn(
                name: "ActualStartDate",
                table: "systematic_review_projects");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "systematic_review_projects");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "systematic_review_projects");

            migrationBuilder.RenameColumn(
                name: "UpdatedByUserId",
                table: "systematic_review_projects",
                newName: "OwnerId");
        }
    }
}
