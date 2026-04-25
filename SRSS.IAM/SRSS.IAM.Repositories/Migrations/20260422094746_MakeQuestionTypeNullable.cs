using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class MakeQuestionTypeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_research_question_question_type_question_type_id",
                table: "research_question");

            migrationBuilder.AlterColumn<Guid>(
                name: "question_type_id",
                table: "research_question",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_research_question_question_type_question_type_id",
                table: "research_question",
                column: "question_type_id",
                principalTable: "question_type",
                principalColumn: "question_type_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_research_question_question_type_question_type_id",
                table: "research_question");

            migrationBuilder.AlterColumn<Guid>(
                name: "question_type_id",
                table: "research_question",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_research_question_question_type_question_type_id",
                table: "research_question",
                column: "question_type_id",
                principalTable: "question_type",
                principalColumn: "question_type_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
