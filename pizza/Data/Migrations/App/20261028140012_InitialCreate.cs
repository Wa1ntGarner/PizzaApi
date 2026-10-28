using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PizzaApi.Data.Migrations.App
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pizzas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    DiameterCm = table.Column<int>(type: "int", nullable: false),
                    IsVegetarian = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pizzas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PizzaId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Pizzas_PizzaId",
                        column: x => x.PizzaId,
                        principalTable: "Pizzas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "Address", "CreatedAt", "CustomerName", "CustomerPhone", "Status", "TotalPrice" },
                values: new object[] { 1, "г. Москва, ул. Пушкина, д. 10, кв. 5", new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), "Иван Петров", "+7-900-000-00-01", "Completed", 1040m });

            migrationBuilder.InsertData(
                table: "Pizzas",
                columns: new[] { "Id", "CreatedAt", "Description", "DiameterCm", "IsAvailable", "IsVegetarian", "Name", "Price" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Томатный соус, моцарелла, базилик", 30, true, true, "Маргарита", 450m },
                    { 2, new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Томатный соус, моцарелла, острая пепперони", 30, true, false, "Пепперони", 590m },
                    { 3, new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Моцарелла, дорблю, чеддер, пармезан", 30, true, true, "Четыре сыра", 620m },
                    { 4, new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Ветчина, бекон, колбаса, говядина", 40, true, false, "Мясная", 720m },
                    { 5, new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Курица, ананас, моцарелла, соус", 35, true, false, "Гавайская", 640m },
                    { 6, new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Перец, шампиньоны, оливки, томаты черри", 30, true, true, "Овощная", 560m },
                    { 7, new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Острый соус, халапеньо, пепперони", 35, false, false, "Дьябло", 680m }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "OrderId", "PizzaId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, 450m },
                    { 2, 1, 2, 1, 590m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PizzaId",
                table: "OrderItems",
                column: "PizzaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pizzas_Name",
                table: "Pizzas",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Pizzas");
        }
    }
}
