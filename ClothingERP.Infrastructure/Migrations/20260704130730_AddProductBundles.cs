using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductBundles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BundleName",
                table: "SalesInvoiceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductBundleId",
                table: "SalesInvoiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductBundle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BundlePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBundle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductBundleItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductBundleId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBundleItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBundleItem_ProductBundle_ProductBundleId",
                        column: x => x.ProductBundleId,
                        principalTable: "ProductBundle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBundleItem_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItems_ProductBundleId",
                table: "SalesInvoiceItems",
                column: "ProductBundleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItem_ProductBundleId",
                table: "ProductBundleItem",
                column: "ProductBundleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItem_ProductVariantId",
                table: "ProductBundleItem",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_ProductBundle_ProductBundleId",
                table: "SalesInvoiceItems",
                column: "ProductBundleId",
                principalTable: "ProductBundle",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_ProductBundle_ProductBundleId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropTable(
                name: "ProductBundleItem");

            migrationBuilder.DropTable(
                name: "ProductBundle");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceItems_ProductBundleId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "BundleName",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "ProductBundleId",
                table: "SalesInvoiceItems");
        }
    }
}
