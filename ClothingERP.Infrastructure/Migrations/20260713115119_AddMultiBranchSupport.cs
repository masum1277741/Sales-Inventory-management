using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiBranchSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Stocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "SalesInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "PurchaseOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "AccountTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMainBranch = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            // ── Seed a default "Main Branch" and backfill existing rows ──
            migrationBuilder.Sql(@"
                INSERT INTO [Branches] ([Code], [Name], [IsMainBranch], [IsActive], [CreatedAt], [IsDeleted])
                VALUES ('MAIN', 'Main Branch', 1, 1, GETUTCDATE(), 0);
            ");

            migrationBuilder.Sql(@"
                UPDATE [Stocks] SET [BranchId] = (SELECT TOP 1 [Id] FROM [Branches] WHERE [Code] = 'MAIN') WHERE [BranchId] = 0;
                UPDATE [SalesInvoices] SET [BranchId] = (SELECT TOP 1 [Id] FROM [Branches] WHERE [Code] = 'MAIN') WHERE [BranchId] = 0;
                UPDATE [PurchaseOrders] SET [BranchId] = (SELECT TOP 1 [Id] FROM [Branches] WHERE [Code] = 'MAIN') WHERE [BranchId] = 0;
                UPDATE [AccountTransactions] SET [BranchId] = (SELECT TOP 1 [Id] FROM [Branches] WHERE [Code] = 'MAIN') WHERE [BranchId] = 0;
            ");

            migrationBuilder.CreateTable(
                name: "StockTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromBranchId = table.Column<int>(type: "int", nullable: false),
                    ToBranchId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedBy = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransfers_Branches_FromBranchId",
                        column: x => x.FromBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransfers_Branches_ToBranchId",
                        column: x => x.ToBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBranches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBranches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTransferId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    RequestedQty = table.Column<int>(type: "int", nullable: false),
                    ReceivedQty = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockTransferItems_StockTransfers_StockTransferId",
                        column: x => x.StockTransferId,
                        principalTable: "StockTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_BranchId",
                table: "Stocks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_BranchId",
                table: "SalesInvoices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BranchId",
                table: "PurchaseOrders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransactions_BranchId",
                table: "AccountTransactions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_ProductVariantId",
                table: "StockTransferItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_StockTransferId",
                table: "StockTransferItems",
                column: "StockTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_FromBranchId",
                table: "StockTransfers",
                column: "FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_ToBranchId",
                table: "StockTransfers",
                column: "ToBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBranches_BranchId",
                table: "UserBranches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBranches_UserId",
                table: "UserBranches",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountTransactions_Branches_BranchId",
                table: "AccountTransactions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Branches_BranchId",
                table: "PurchaseOrders",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Branches_BranchId",
                table: "SalesInvoices",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Branches_BranchId",
                table: "Stocks",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountTransactions_Branches_BranchId",
                table: "AccountTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Branches_BranchId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Branches_BranchId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Branches_BranchId",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "StockTransferItems");

            migrationBuilder.DropTable(
                name: "UserBranches");

            migrationBuilder.DropTable(
                name: "StockTransfers");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_BranchId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_BranchId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_BranchId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_AccountTransactions_BranchId",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AccountTransactions");
        }
    }
}