using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RelationentityofNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "navigation_url",
                table: "notifications");

            migrationBuilder.AddColumn<int>(
                name: "entity_type",
                table: "notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "related_entity_id",
                table: "notifications",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "entity_type",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "related_entity_id",
                table: "notifications");

            migrationBuilder.AddColumn<string>(
                name: "navigation_url",
                table: "notifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
