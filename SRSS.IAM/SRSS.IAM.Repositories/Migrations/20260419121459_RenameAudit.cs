using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RenameAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "audit_logs");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "audit_logs",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "audit_logs",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Importance",
                table: "audit_logs",
                newName: "importance");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "audit_logs",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "audit_logs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "audit_logs",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "audit_logs",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UserAgent",
                table: "audit_logs",
                newName: "user_agent");

            migrationBuilder.RenameColumn(
                name: "ResourceType",
                table: "audit_logs",
                newName: "resource_type");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "audit_logs",
                newName: "resource_id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "audit_logs",
                newName: "project_id");

            migrationBuilder.RenameColumn(
                name: "OldValue",
                table: "audit_logs",
                newName: "old_value");

            migrationBuilder.RenameColumn(
                name: "NewValue",
                table: "audit_logs",
                newName: "new_value");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "audit_logs",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "audit_logs",
                newName: "ip_address");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "audit_logs",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AffectedColumns",
                table: "audit_logs",
                newName: "affected_columns");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "audit_logs",
                newName: "action_type");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs");

            migrationBuilder.RenameTable(
                name: "audit_logs",
                newName: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "AuditLogs",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "AuditLogs",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "importance",
                table: "AuditLogs",
                newName: "Importance");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "AuditLogs",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AuditLogs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "AuditLogs",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AuditLogs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "user_agent",
                table: "AuditLogs",
                newName: "UserAgent");

            migrationBuilder.RenameColumn(
                name: "resource_type",
                table: "AuditLogs",
                newName: "ResourceType");

            migrationBuilder.RenameColumn(
                name: "resource_id",
                table: "AuditLogs",
                newName: "ResourceId");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "AuditLogs",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "old_value",
                table: "AuditLogs",
                newName: "OldValue");

            migrationBuilder.RenameColumn(
                name: "new_value",
                table: "AuditLogs",
                newName: "NewValue");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "AuditLogs",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "ip_address",
                table: "AuditLogs",
                newName: "IpAddress");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "AuditLogs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "affected_columns",
                table: "AuditLogs",
                newName: "AffectedColumns");

            migrationBuilder.RenameColumn(
                name: "action_type",
                table: "AuditLogs",
                newName: "ActionType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs",
                column: "Id");
        }
    }
}
