using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductionManagement.Migrations
{
    [Migration("20260524000000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", nullable: false),
                    MinimalStock = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Specifications = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    MinimalStock = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionTimePerUnit = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    EfficiencyFactor = table.Column<float>(type: "REAL", nullable: false),
                    CurrentWorkOrderId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductMaterials",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityNeeded = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMaterials", x => new { x.ProductId, x.MaterialId });
                    table.ForeignKey(name: "FK_ProductMaterials_Materials_MaterialId", column: x => x.MaterialId, principalTable: "Materials", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_ProductMaterials_Products_ProductId", column: x => x.ProductId, principalTable: "Products", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionLineId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EstimatedEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ProgressPercent = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(name: "FK_WorkOrders_ProductionLines_ProductionLineId", column: x => x.ProductionLineId, principalTable: "ProductionLines", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(name: "FK_WorkOrders_Products_ProductId", column: x => x.ProductId, principalTable: "Products", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(table: "Materials", columns: new[] { "Id", "MinimalStock", "Name", "Quantity", "UnitOfMeasure" },
                values: new object[,] {
                    { 1, 100m, "Сталь листовая", 500m, "кг" },
                    { 2, 500m, "Болты М8", 1200m, "шт" },
                    { 3, 50m, "Краска синяя", 30m, "литр" },
                    { 4, 100m, "Пластик ABS", 80m, "кг" }
                });

            migrationBuilder.InsertData(table: "Products", columns: new[] { "Id", "Category", "Description", "MinimalStock", "Name", "ProductionTimePerUnit", "Specifications" },
                values: new object[,] {
                    { 1, "Корпуса", "Стальной корпус серии A", 10, "Корпус машины А1", 120, null },
                    { 2, "Электроника", "Пластиковая панель", 5, "Панель управления", 60, null }
                });

            migrationBuilder.InsertData(table: "ProductionLines", columns: new[] { "Id", "CurrentWorkOrderId", "EfficiencyFactor", "Name", "Status" },
                values: new object[,] {
                    { 1, null, 1f, "Линия А", "Active" },
                    { 2, null, 1.2f, "Линия Б", "Stopped" },
                    { 3, null, 0.8f, "Линия В", "Active" }
                });

            migrationBuilder.InsertData(table: "ProductMaterials", columns: new[] { "MaterialId", "ProductId", "QuantityNeeded" },
                values: new object[,] {
                    { 1, 1, 15m },
                    { 2, 1, 24m },
                    { 4, 2, 2m }
                });

            migrationBuilder.InsertData(table: "WorkOrders",
                columns: new[] { "Id", "EstimatedEndDate", "ProductId", "ProductionLineId", "ProgressPercent", "Quantity", "StartDate", "Status" },
                values: new object[,] {
                    { 1, new DateTime(2026, 5, 27, 0, 0, 0), 1, 1, 40, 5, new DateTime(2026, 5, 24, 0, 0, 0), "InProgress" },
                    { 2, new DateTime(2026, 5, 26, 0, 0, 0), 2, null, 0, 10, new DateTime(2026, 5, 25, 0, 0, 0), "Pending" }
                });

            migrationBuilder.CreateIndex(name: "IX_ProductMaterials_MaterialId", table: "ProductMaterials", column: "MaterialId");
            migrationBuilder.CreateIndex(name: "IX_WorkOrders_ProductId", table: "WorkOrders", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_WorkOrders_ProductionLineId", table: "WorkOrders", column: "ProductionLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProductMaterials");
            migrationBuilder.DropTable(name: "WorkOrders");
            migrationBuilder.DropTable(name: "Materials");
            migrationBuilder.DropTable(name: "ProductionLines");
            migrationBuilder.DropTable(name: "Products");
        }
    }
}
