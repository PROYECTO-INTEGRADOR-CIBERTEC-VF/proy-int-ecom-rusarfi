using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RusarfiServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
INSERT INTO Categories (Name, CreatedAtUtc)
SELECT DISTINCT LTRIM(RTRIM(Category)), GETUTCDATE()
FROM Products
WHERE Category IS NOT NULL AND LTRIM(RTRIM(Category)) <> '';
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Accesorios')
BEGIN
    INSERT INTO Categories (Name, CreatedAtUtc)
    VALUES (N'Accesorios', GETUTCDATE())
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Sin categoria')
BEGIN
    INSERT INTO Categories (Name, CreatedAtUtc)
    VALUES (N'Sin categoria', GETUTCDATE())
END
");

            migrationBuilder.Sql(@"
UPDATE p
SET CategoryId = c.Id
FROM Products p
INNER JOIN Categories c
    ON c.Name = LTRIM(RTRIM(p.Category));
");

            migrationBuilder.Sql(@"
UPDATE p
SET CategoryId = c.Id
FROM Products p
CROSS JOIN Categories c
WHERE c.Name = N'Sin categoria'
  AND (p.CategoryId IS NULL OR p.CategoryId = 0);
");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE p
SET Category = c.Name
FROM Products p
INNER JOIN Categories c
    ON p.CategoryId = c.Id;
");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
