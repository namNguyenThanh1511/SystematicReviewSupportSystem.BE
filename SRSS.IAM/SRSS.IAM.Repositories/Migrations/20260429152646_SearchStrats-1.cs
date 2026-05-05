using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SearchStrats1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "comparison_keywords",
                table: "search_strategy",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "context_keywords",
                table: "search_strategy",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "intervention_keywords",
                table: "search_strategy",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "outcome_keywords",
                table: "search_strategy",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "population_keywords",
                table: "search_strategy",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "comparison_keywords",
                table: "search_strategy");

            migrationBuilder.DropColumn(
                name: "context_keywords",
                table: "search_strategy");

            migrationBuilder.DropColumn(
                name: "intervention_keywords",
                table: "search_strategy");

            migrationBuilder.DropColumn(
                name: "outcome_keywords",
                table: "search_strategy");

            migrationBuilder.DropColumn(
                name: "population_keywords",
                table: "search_strategy");
        }
    }
}
