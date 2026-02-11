using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class initialpaper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "abstract_language",
                table: "papers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "access_type",
                table: "papers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conference_country",
                table: "papers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "conference_end_date",
                table: "papers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conference_location",
                table: "papers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conference_name",
                table: "papers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "conference_start_date",
                table: "papers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "conference_year",
                table: "papers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "current_selection_status",
                table: "papers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "full_text_available",
                table: "papers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "import_batch_id",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "imported_at",
                table: "papers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imported_by",
                table: "papers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "internal_notes",
                table: "papers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_included_final",
                table: "papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "issue",
                table: "papers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "journal_e_issn",
                table: "papers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "journal_issn",
                table: "papers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "journal_publisher",
                table: "papers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "papers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_decision_at",
                table: "papers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pages",
                table: "papers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdf_url",
                table: "papers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "publication_date",
                table: "papers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "publication_type",
                table: "papers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "publication_year_int",
                table: "papers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "publisher",
                table: "papers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "raw_reference",
                table: "papers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "papers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_record_id",
                table: "papers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "volume",
                table: "papers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_papers_doi",
                table: "papers",
                column: "doi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_papers_doi",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "abstract_language",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "access_type",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "conference_country",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "conference_end_date",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "conference_location",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "conference_name",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "conference_start_date",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "conference_year",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "current_selection_status",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "full_text_available",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "import_batch_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "imported_at",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "imported_by",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "internal_notes",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "is_included_final",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "issue",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "journal_e_issn",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "journal_issn",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "journal_publisher",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "language",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "last_decision_at",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "pages",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "pdf_url",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "publication_date",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "publication_type",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "publication_year_int",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "publisher",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "raw_reference",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "source",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "source_record_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "volume",
                table: "papers");
        }
    }
}
