using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartReorderSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AverageLeadTimeDays",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReorderSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalysisPeriodDays = table.Column<int>(type: "int", nullable: false),
                    DefaultLeadTimeDays = table.Column<int>(type: "int", nullable: false),
                    SafetyStockDays = table.Column<int>(type: "int", nullable: false),
                    MinDailyVelocity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReorderSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReorderSettings");

            migrationBuilder.DropColumn(
                name: "AverageLeadTimeDays",
                table: "Suppliers");
        }
    }
}
