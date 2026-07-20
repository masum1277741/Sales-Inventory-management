using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEcommerceStorefront : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DigitalPaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GatewayPaymentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GatewayTrxId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountBDT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesInvoiceId = table.Column<int>(type: "int", nullable: true),
                    CustomerMsisdn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InitiatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RawResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalPaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalPaymentTransactions_SalesInvoices_SalesInvoiceId",
                        column: x => x.SalesInvoiceId,
                        principalTable: "SalesInvoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "onlineOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuestPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubtotalUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingFeeUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DigitalPaymentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FulfillmentBranchId = table.Column<int>(type: "int", nullable: true),
                    SalesInvoiceId = table.Column<int>(type: "int", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_onlineOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_onlineOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StorefrontSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsStoreEnabled = table.Column<bool>(type: "bit", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreTagline = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlatShippingFeeUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FreeShippingThresholdUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CodEnabled = table.Column<bool>(type: "bit", nullable: false),
                    BkashEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NagadEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FulfillmentBranchId = table.Column<int>(type: "int", nullable: false),
                    BannerImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnnouncementText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorefrontSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OnlineOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnlineOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPriceUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotalUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineOrderItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineOrderItems_onlineOrders_OnlineOrderId",
                        column: x => x.OnlineOrderId,
                        principalTable: "onlineOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalPaymentTransactions_SalesInvoiceId",
                table: "DigitalPaymentTransactions",
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrderItems_OnlineOrderId",
                table: "OnlineOrderItems",
                column: "OnlineOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrderItems_ProductVariantId",
                table: "OnlineOrderItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_onlineOrders_CustomerId",
                table: "onlineOrders",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalPaymentTransactions");

            migrationBuilder.DropTable(
                name: "OnlineOrderItems");

            migrationBuilder.DropTable(
                name: "StorefrontSettings");

            migrationBuilder.DropTable(
                name: "onlineOrders");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Customers");
        }
    }
}
