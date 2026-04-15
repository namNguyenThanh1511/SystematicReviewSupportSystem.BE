using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addpapersearchsoureid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "search_source_id",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_papers_search_source_id",
                table: "papers",
                column: "search_source_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_papers_search_source_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "search_source_id",
                table: "papers");
        }
    }
}
