using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Relationship1to1forprocessandprotocol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "protocol_id",
                table: "review_processes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_review_processes_protocol_id",
                table: "review_processes",
                column: "protocol_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_review_processes_review_protocol_protocol_id",
                table: "review_processes",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_review_processes_review_protocol_protocol_id",
                table: "review_processes");

            migrationBuilder.DropIndex(
                name: "IX_review_processes_protocol_id",
                table: "review_processes");

            migrationBuilder.DropColumn(
                name: "protocol_id",
                table: "review_processes");
        }
    }
}
