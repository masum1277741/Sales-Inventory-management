using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftCardsAndStoreCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductBundleItem_ProductBundle_ProductBundleId",
                table: "ProductBundleItem");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductBundleItem_ProductVariants_ProductVariantId",
                table: "ProductBundleItem");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_ProductBundle_ProductBundleId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductBundleItem",
                table: "ProductBundleItem");

            migrationBuilder.DropIndex(
                name: "IX_ProductBundleItem_ProductBundleId",
                table: "ProductBundleItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductBundle",
                table: "ProductBundle");

            migrationBuilder.RenameTable(
                name: "ProductBundleItem",
                newName: "ProductBundleItems");

            migrationBuilder.RenameTable(
                name: "ProductBundle",
                newName: "ProductBundles");

            migrationBuilder.RenameIndex(
                name: "IX_ProductBundleItem_ProductVariantId",
                table: "ProductBundleItems",
                newName: "IX_ProductBundleItems_ProductVariantId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductBundles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "ProductBundles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProductBundles",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductBundleItems",
                table: "ProductBundleItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductBundles",
                table: "ProductBundles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItems_ProductBundleId_ProductVariantId",
                table: "ProductBundleItems",
                columns: new[] { "ProductBundleId", "ProductVariantId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBundleItems_ProductBundles_ProductBundleId",
                table: "ProductBundleItems",
                column: "ProductBundleId",
                principalTable: "ProductBundles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBundleItems_ProductVariants_ProductVariantId",
                table: "ProductBundleItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_ProductBundles_ProductBundleId",
                table: "SalesInvoiceItems",
                column: "ProductBundleId",
                principalTable: "ProductBundles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductBundleItems_ProductBundles_ProductBundleId",
                table: "ProductBundleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductBundleItems_ProductVariants_ProductVariantId",
                table: "ProductBundleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_ProductBundles_ProductBundleId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductBundles",
                table: "ProductBundles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductBundleItems",
                table: "ProductBundleItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductBundleItems_ProductBundleId_ProductVariantId",
                table: "ProductBundleItems");

            migrationBuilder.RenameTable(
                name: "ProductBundles",
                newName: "ProductBundle");

            migrationBuilder.RenameTable(
                name: "ProductBundleItems",
                newName: "ProductBundleItem");

            migrationBuilder.RenameIndex(
                name: "IX_ProductBundleItems_ProductVariantId",
                table: "ProductBundleItem",
                newName: "IX_ProductBundleItem_ProductVariantId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductBundle",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "ProductBundle",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProductBundle",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductBundle",
                table: "ProductBundle",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductBundleItem",
                table: "ProductBundleItem",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBundleItem_ProductBundleId",
                table: "ProductBundleItem",
                column: "ProductBundleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBundleItem_ProductBundle_ProductBundleId",
                table: "ProductBundleItem",
                column: "ProductBundleId",
                principalTable: "ProductBundle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBundleItem_ProductVariants_ProductVariantId",
                table: "ProductBundleItem",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_ProductBundle_ProductBundleId",
                table: "SalesInvoiceItems",
                column: "ProductBundleId",
                principalTable: "ProductBundle",
                principalColumn: "Id");
        }
    }
}
