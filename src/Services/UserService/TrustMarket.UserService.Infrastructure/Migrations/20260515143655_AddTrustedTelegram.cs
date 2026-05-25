using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrustedTelegram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TrustedContactTelegramId",
                schema: "users",
                table: "Users",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrustedContactTelegramId",
                schema: "users",
                table: "Users");
        }
    }
}
