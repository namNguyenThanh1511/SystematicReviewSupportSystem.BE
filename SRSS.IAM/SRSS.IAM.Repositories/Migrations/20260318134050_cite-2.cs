using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class cite2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_paper_citations_SourcePaperId_TargetPaperId",
                table: "paper_citations",
                columns: new[] { "SourcePaperId", "TargetPaperId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_paper_citations_SourcePaperId_TargetPaperId",
                table: "paper_citations");
        }
    }
}
