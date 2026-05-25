using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryItem",
                schema: "catalog",
                table: "Advertisements",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryLabel",
                schema: "catalog",
                table: "Advertisements",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategorySub",
                schema: "catalog",
                table: "Advertisements",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_Category_CategorySub",
                schema: "catalog",
                table: "Advertisements",
                columns: new[] { "Category", "CategorySub" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Advertisements_Category_CategorySub",
                schema: "catalog",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "CategoryItem",
                schema: "catalog",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "CategoryLabel",
                schema: "catalog",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "CategorySub",
                schema: "catalog",
                table: "Advertisements");
        }
    }
}
