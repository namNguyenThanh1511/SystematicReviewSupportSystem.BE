using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class jsonbflowrec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "metadata_json",
                table: "prisma_flow_records",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "metadata_json",
                table: "prisma_flow_records");
        }
    }
}
