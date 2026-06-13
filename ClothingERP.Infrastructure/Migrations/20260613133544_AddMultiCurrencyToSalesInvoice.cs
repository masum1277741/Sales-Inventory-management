using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCurrencyToSalesInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateBDT",
                table: "SalesInvoices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateMVR",
                table: "SalesInvoices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountBDT",
                table: "SalesInvoices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountMVR",
                table: "SalesInvoices",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExchangeRateBDT",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "ExchangeRateMVR",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "TotalAmountBDT",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "TotalAmountMVR",
                table: "SalesInvoices");
        }
    }
}
