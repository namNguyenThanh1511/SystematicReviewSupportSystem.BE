using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddExtractionCommentLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvidenceCoordinates",
                table: "extracted_data_value",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNotReported",
                table: "extracted_data_value",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExtractedDataAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractionProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatrixColumnId = table.Column<Guid>(type: "uuid", nullable: true),
                    MatrixRowIndex = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldValue = table.Column<string>(type: "text", nullable: false),
                    NewValue = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractedDataAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractedDataAuditLogs_ExtractionMatrixColumns_MatrixColumn~",
                        column: x => x.MatrixColumnId,
                        principalTable: "ExtractionMatrixColumns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExtractedDataAuditLogs_data_extraction_process_ExtractionPr~",
                        column: x => x.ExtractionProcessId,
                        principalTable: "data_extraction_process",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtractedDataAuditLogs_extraction_field_FieldId",
                        column: x => x.FieldId,
                        principalTable: "extraction_field",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtractedDataAuditLogs_papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtractedDataAuditLogs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "extraction_comment",
                columns: table => new
                {
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    extraction_paper_task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_id = table.Column<Guid>(type: "uuid", nullable: false),
                    matrix_column_id = table.Column<Guid>(type: "uuid", nullable: true),
                    matrix_row_index = table.Column<int>(type: "integer", nullable: true),
                    thread_owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraction_comment", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_extraction_comment_ExtractionMatrixColumns_matrix_column_id",
                        column: x => x.matrix_column_id,
                        principalTable: "ExtractionMatrixColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extraction_comment_extraction_field_field_id",
                        column: x => x.field_id,
                        principalTable: "extraction_field",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extraction_comment_extraction_paper_task_extraction_paper_t~",
                        column: x => x.extraction_paper_task_id,
                        principalTable: "extraction_paper_task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extraction_comment_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedDataAuditLogs_ExtractionProcessId",
                table: "ExtractedDataAuditLogs",
                column: "ExtractionProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedDataAuditLogs_FieldId",
                table: "ExtractedDataAuditLogs",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedDataAuditLogs_MatrixColumnId",
                table: "ExtractedDataAuditLogs",
                column: "MatrixColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedDataAuditLogs_PaperId",
                table: "ExtractedDataAuditLogs",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedDataAuditLogs_UserId",
                table: "ExtractedDataAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_comment_extraction_paper_task_id",
                table: "extraction_comment",
                column: "extraction_paper_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_comment_field_id",
                table: "extraction_comment",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_comment_matrix_column_id",
                table: "extraction_comment",
                column: "matrix_column_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_comment_user_id",
                table: "extraction_comment",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractedDataAuditLogs");

            migrationBuilder.DropTable(
                name: "extraction_comment");

            migrationBuilder.DropColumn(
                name: "EvidenceCoordinates",
                table: "extracted_data_value");

            migrationBuilder.DropColumn(
                name: "IsNotReported",
                table: "extracted_data_value");
        }
    }
}
