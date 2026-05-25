using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.ChatService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrustedTelegramToViewing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProposerTrustedTelegramId",
                schema: "chat",
                table: "ViewingRequests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ResponderTrustedTelegramId",
                schema: "chat",
                table: "ViewingRequests",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProposerTrustedTelegramId",
                schema: "chat",
                table: "ViewingRequests");

            migrationBuilder.DropColumn(
                name: "ResponderTrustedTelegramId",
                schema: "chat",
                table: "ViewingRequests");
        }
    }
}
