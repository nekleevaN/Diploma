using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.FinanceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deliveries",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientCityRef = table.Column<string>(type: "text", nullable: true),
                    RecipientCityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RecipientWarehouseRef = table.Column<string>(type: "text", nullable: true),
                    RecipientWarehouseAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RecipientPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SenderCityRef = table.Column<string>(type: "text", nullable: true),
                    SenderCityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SenderWarehouseRef = table.Column<string>(type: "text", nullable: true),
                    SenderWarehouseAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SenderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SenderPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TTN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TrackingStatus = table.Column<string>(type: "text", nullable: true),
                    TrackingStatusDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderId",
                schema: "finance",
                table: "Deliveries",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deliveries",
                schema: "finance");
        }
    }
}
