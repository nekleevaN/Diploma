using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.ChatService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdTitle",
                schema: "chat",
                table: "Chats",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "Оголошення");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdTitle",
                schema: "chat",
                table: "Chats");
        }
    }
}
