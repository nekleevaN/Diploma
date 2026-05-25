using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonobankSubMerchantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MonobankSubMerchantId",
                schema: "users",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonobankSubMerchantId",
                schema: "users",
                table: "Users");
        }
    }
}
