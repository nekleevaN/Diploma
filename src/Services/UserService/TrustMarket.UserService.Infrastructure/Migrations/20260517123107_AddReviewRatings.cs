using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BuyerRating",
                schema: "users",
                table: "Users",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BuyerReviewCount",
                schema: "users",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SellerRating",
                schema: "users",
                table: "Users",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SellerReviewCount",
                schema: "users",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerRating",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BuyerReviewCount",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerRating",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SellerReviewCount",
                schema: "users",
                table: "Users");
        }
    }
}
