using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class refactorsource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "search_source",
                table: "search_executions");

            migrationBuilder.RenameColumn(
                name: "source_id",
                table: "search_source",
                newName: "id");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "search_source",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "master_source_id",
                table: "search_source",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "search_source_id",
                table: "search_executions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "master_search_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    base_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_master_search_sources", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_search_source_master_source_id",
                table: "search_source",
                column: "master_source_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_executions_search_source_id",
                table: "search_executions",
                column: "search_source_id");

            migrationBuilder.AddForeignKey(
                name: "FK_search_executions_search_source_search_source_id",
                table: "search_executions",
                column: "search_source_id",
                principalTable: "search_source",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_search_source_master_search_sources_master_source_id",
                table: "search_source",
                column: "master_source_id",
                principalTable: "master_search_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_search_executions_search_source_search_source_id",
                table: "search_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_search_source_master_search_sources_master_source_id",
                table: "search_source");

            migrationBuilder.DropTable(
                name: "master_search_sources");

            migrationBuilder.DropIndex(
                name: "IX_search_source_master_source_id",
                table: "search_source");

            migrationBuilder.DropIndex(
                name: "IX_search_executions_search_source_id",
                table: "search_executions");

            migrationBuilder.DropColumn(
                name: "master_source_id",
                table: "search_source");

            migrationBuilder.DropColumn(
                name: "search_source_id",
                table: "search_executions");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "search_source",
                newName: "source_id");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "search_source",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "search_source",
                table: "search_executions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
