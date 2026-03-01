using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddDeduplicationReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "review_status",
                table: "deduplication_results",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reviewed_at",
                table: "deduplication_results",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reviewed_by",
                table: "deduplication_results",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_review_status",
                table: "deduplication_results",
                column: "review_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_deduplication_results_review_status",
                table: "deduplication_results");

            migrationBuilder.DropColumn(
                name: "review_status",
                table: "deduplication_results");

            migrationBuilder.DropColumn(
                name: "reviewed_at",
                table: "deduplication_results");

            migrationBuilder.DropColumn(
                name: "reviewed_by",
                table: "deduplication_results");
        }
    }
}
