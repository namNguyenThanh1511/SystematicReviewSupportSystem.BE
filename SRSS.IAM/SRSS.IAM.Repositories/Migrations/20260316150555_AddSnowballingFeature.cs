using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSnowballingFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginPaperId",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "papers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CandidatePapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginPaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Authors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PublicationYear = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DOI = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RawReference = table.Column<string>(type: "text", nullable: true),
                    NormalizedReference = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatePapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidatePapers_papers_OriginPaperId",
                        column: x => x.OriginPaperId,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidatePapers_systematic_review_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePapers_OriginPaperId",
                table: "CandidatePapers",
                column: "OriginPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePapers_ProjectId",
                table: "CandidatePapers",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidatePapers");

            migrationBuilder.DropColumn(
                name: "OriginPaperId",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "papers");
        }
    }
}
