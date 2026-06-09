using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuditLogProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ActionDate",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ActionDate",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsSuccess",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "TableName",
                table: "AuditLogs",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "AuditLogs",
                newName: "ActionType");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "AuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EntityName",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "AuditLogs",
                newName: "TableName");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "AuditLogs",
                newName: "Action");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActionDate",
                table: "AuditLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "AuditLogs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccess",
                table: "AuditLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecordId",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActionDate",
                table: "AuditLogs",
                column: "ActionDate");
        }
    }
}
