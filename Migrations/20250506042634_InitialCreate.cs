using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceProcurementSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityID);
                });

            migrationBuilder.CreateTable(
                name: "Commodities",
                columns: table => new
                {
                    CommodityID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Commodity_Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commodities", x => x.CommodityID);
                });

            migrationBuilder.CreateTable(
                name: "MasterAgreements",
                columns: table => new
                {
                    Master_Agreement = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Contract_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Award_Date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterAgreements", x => x.Master_Agreement);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Vendor_Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Vendor_Code);
                });

            migrationBuilder.CreateTable(
                name: "AnnualReports",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityID = table.Column<int>(type: "int", nullable: false),
                    Vendor_Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    SaleAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnualReports", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AnnualReports_Cities_CityID",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "CityID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnnualReports_Vendors_Vendor_Code",
                        column: x => x.Vendor_Code,
                        principalTable: "Vendors",
                        principalColumn: "Vendor_Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Purchase_Order = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Vendor_Code = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Master_Agreement = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Purchase_Order);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_MasterAgreements_Master_Agreement",
                        column: x => x.Master_Agreement,
                        principalTable: "MasterAgreements",
                        principalColumn: "Master_Agreement");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Vendors_Vendor_Code",
                        column: x => x.Vendor_Code,
                        principalTable: "Vendors",
                        principalColumn: "Vendor_Code");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLines",
                columns: table => new
                {
                    Purchase_Order = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommodityID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Line_Item_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity_Ordered = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit_Of_Measure_Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit_Of_Measure_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Line_Item_Total_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderLines", x => new { x.Purchase_Order, x.CommodityID });
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_Commodities_CommodityID",
                        column: x => x.CommodityID,
                        principalTable: "Commodities",
                        principalColumn: "CommodityID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_PurchaseOrders_Purchase_Order",
                        column: x => x.Purchase_Order,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Purchase_Order",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnualReports_CityID",
                table: "AnnualReports",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_AnnualReports_Vendor_Code",
                table: "AnnualReports",
                column: "Vendor_Code");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_CommodityID",
                table: "PurchaseOrderLines",
                column: "CommodityID");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Master_Agreement",
                table: "PurchaseOrders",
                column: "Master_Agreement");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Vendor_Code",
                table: "PurchaseOrders",
                column: "Vendor_Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnualReports");

            migrationBuilder.DropTable(
                name: "PurchaseOrderLines");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Commodities");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "MasterAgreements");

            migrationBuilder.DropTable(
                name: "Vendors");
        }
    }
}
