using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProtocolReviewerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_protocol_evaluation_protocol_reviewer_reviewer_id",
                table: "protocol_evaluation");

            migrationBuilder.AddForeignKey(
                name: "FK_protocol_evaluation_users_reviewer_id",
                table: "protocol_evaluation",
                column: "reviewer_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_protocol_evaluation_users_reviewer_id",
                table: "protocol_evaluation");

            migrationBuilder.AddForeignKey(
                name: "FK_protocol_evaluation_protocol_reviewer_reviewer_id",
                table: "protocol_evaluation",
                column: "reviewer_id",
                principalTable: "protocol_reviewer",
                principalColumn: "reviewer_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
