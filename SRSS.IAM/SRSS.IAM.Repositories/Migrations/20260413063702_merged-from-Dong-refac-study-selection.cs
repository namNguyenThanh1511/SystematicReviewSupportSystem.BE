using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class mergedfromDongrefacstudyselection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExclusionReasonCode",
                table: "screening_resolutions");

            migrationBuilder.DropColumn(
                name: "exclusion_reason_code",
                table: "screening_decisions");

            migrationBuilder.DropColumn(
                name: "reviewer_notes",
                table: "screening_decisions");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<Guid>(
                name: "exclusion_reason_id",
                table: "screening_resolutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "exclusion_reason_id",
                table: "screening_decisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "exclusion_reason_libraries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exclusion_reason_libraries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_exclusion_reasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    library_reason_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_exclusion_reasons", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_exclusion_reasons_exclusion_reason_librarie~",
                        column: x => x.library_reason_id,
                        principalTable: "exclusion_reason_libraries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_study_selection_exclusion_reasons_study_selection_processes~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_exclusion_reason_id",
                table: "screening_resolutions",
                column: "exclusion_reason_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_exclusion_reason_id",
                table: "screening_decisions",
                column: "exclusion_reason_id");

            migrationBuilder.CreateIndex(
                name: "ix_exclusion_reason_libraries_code",
                table: "exclusion_reason_libraries",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exclusion_reason_libraries_name",
                table: "exclusion_reason_libraries",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_study_selection_exclusion_reasons_code",
                table: "study_selection_exclusion_reasons",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "idx_study_selection_exclusion_reasons_process_id",
                table: "study_selection_exclusion_reasons",
                column: "study_selection_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_exclusion_reasons_library_reason_id",
                table: "study_selection_exclusion_reasons",
                column: "library_reason_id");

            migrationBuilder.CreateIndex(
                name: "ux_study_selection_exclusion_reasons_process_code",
                table: "study_selection_exclusion_reasons",
                columns: new[] { "study_selection_process_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_study_selection_exclusion_reasons_process_library",
                table: "study_selection_exclusion_reasons",
                columns: new[] { "study_selection_process_id", "library_reason_id" },
                unique: true,
                filter: "library_reason_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_study_selection_exclusion_reasons_process_name",
                table: "study_selection_exclusion_reasons",
                columns: new[] { "study_selection_process_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_screening_decisions_study_selection_exclusion_reasons_exclu~",
                table: "screening_decisions",
                column: "exclusion_reason_id",
                principalTable: "study_selection_exclusion_reasons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_screening_resolutions_study_selection_exclusion_reasons_exc~",
                table: "screening_resolutions",
                column: "exclusion_reason_id",
                principalTable: "study_selection_exclusion_reasons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_screening_decisions_study_selection_exclusion_reasons_exclu~",
                table: "screening_decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_screening_resolutions_study_selection_exclusion_reasons_exc~",
                table: "screening_resolutions");

            migrationBuilder.DropTable(
                name: "study_selection_exclusion_reasons");

            migrationBuilder.DropTable(
                name: "exclusion_reason_libraries");

            migrationBuilder.DropIndex(
                name: "IX_screening_resolutions_exclusion_reason_id",
                table: "screening_resolutions");

            migrationBuilder.DropIndex(
                name: "IX_screening_decisions_exclusion_reason_id",
                table: "screening_decisions");

            migrationBuilder.DropColumn(
                name: "exclusion_reason_id",
                table: "screening_resolutions");

            migrationBuilder.DropColumn(
                name: "exclusion_reason_id",
                table: "screening_decisions");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<int>(
                name: "ExclusionReasonCode",
                table: "screening_resolutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "exclusion_reason_code",
                table: "screening_decisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reviewer_notes",
                table: "screening_decisions",
                type: "text",
                nullable: true);
        }
    }
}
