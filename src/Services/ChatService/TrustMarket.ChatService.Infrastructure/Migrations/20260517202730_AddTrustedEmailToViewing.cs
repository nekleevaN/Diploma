using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.ChatService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrustedEmailToViewing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProposerTrustedEmail",
                schema: "chat",
                table: "ViewingRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponderTrustedEmail",
                schema: "chat",
                table: "ViewingRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProposerTrustedEmail",
                schema: "chat",
                table: "ViewingRequests");

            migrationBuilder.DropColumn(
                name: "ResponderTrustedEmail",
                schema: "chat",
                table: "ViewingRequests");
        }
    }
}
