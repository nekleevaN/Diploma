using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "catalog",
                table: "Advertisements",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationAddress",
                schema: "catalog",
                table: "Advertisements",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "catalog",
                table: "Advertisements",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "catalog",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "LocationAddress",
                schema: "catalog",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "catalog",
                table: "Advertisements");
        }
    }
}
