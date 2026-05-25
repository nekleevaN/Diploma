using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                schema: "catalog",
                table: "Advertisements",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "catalog",
                table: "Advertisements",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                schema: "catalog",
                table: "Advertisements",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                schema: "catalog",
                table: "Advertisements",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                schema: "catalog",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Color",
                schema: "catalog",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Condition",
                schema: "catalog",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Size",
                schema: "catalog",
                table: "Advertisements");
        }
    }
}
