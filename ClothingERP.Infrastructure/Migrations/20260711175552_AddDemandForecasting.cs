using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDemandForecasting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForecastSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalysisPeriodDays = table.Column<int>(type: "int", nullable: false),
                    ForecastHorizonDays = table.Column<int>(type: "int", nullable: false),
                    Alpha = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Beta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Gamma = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinDataPointsRequired = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastSettings");
        }
    }
}
