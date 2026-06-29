using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
        name: "ActionName",
        table: "AuditLogs",
        newName: "ModuleName");

            migrationBuilder.RenameColumn(
                name: "AuditId",
                table: "AuditLogs",
                newName: "AuditLogId");

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
        name: "ActionType",
        table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "ModuleName",
                table: "AuditLogs",
                newName: "ActionName");

            migrationBuilder.RenameColumn(
                name: "AuditLogId",
                table: "AuditLogs",
                newName: "AuditId");
        }
    }
}
