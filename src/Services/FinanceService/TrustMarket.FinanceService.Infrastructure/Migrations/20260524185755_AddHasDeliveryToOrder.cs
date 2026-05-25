using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.FinanceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHasDeliveryToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasDelivery",
                schema: "finance",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Orders_OrderId",
                schema: "finance",
                table: "Deliveries",
                column: "OrderId",
                principalSchema: "finance",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Orders_OrderId",
                schema: "finance",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "HasDelivery",
                schema: "finance",
                table: "Orders");
        }
    }
}
