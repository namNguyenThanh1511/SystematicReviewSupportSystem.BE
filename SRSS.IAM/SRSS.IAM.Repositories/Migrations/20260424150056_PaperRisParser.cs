using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PaperRisParser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookTitle",
                table: "papers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConferenceDate",
                table: "papers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryTitle",
                table: "papers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookTitle",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "ConferenceDate",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "SecondaryTitle",
                table: "papers");
        }
    }
}
