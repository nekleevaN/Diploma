using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.ChatService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViewingRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewingRequests",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdvertisementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocationAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ProposedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FollowUpSent = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpAction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewingRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewingRequests_ChatId",
                schema: "chat",
                table: "ViewingRequests",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewingRequests",
                schema: "chat");
        }
    }
}
