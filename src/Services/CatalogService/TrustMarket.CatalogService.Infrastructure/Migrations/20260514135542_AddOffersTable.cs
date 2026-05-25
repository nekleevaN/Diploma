using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOffersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Offers",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdvertisementId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OfferedPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CounterPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SellerNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_AdvertisementId",
                schema: "catalog",
                table: "Offers",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_BuyerId",
                schema: "catalog",
                table: "Offers",
                column: "BuyerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Offers",
                schema: "catalog");
        }
    }
}
