using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.ReviewService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reviews");

            migrationBuilder.CreateTable(
                name: "Reviews",
                schema: "reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevieweeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DescriptionAccuracy = table.Column<int>(type: "integer", nullable: true),
                    ShippingSpeed = table.Column<int>(type: "integer", nullable: true),
                    Communication = table.Column<int>(type: "integer", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EditableUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RevieweeId_Status",
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "RevieweeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId_RevieweeId_OrderId",
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "ReviewerId", "RevieweeId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId_Status",
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "ReviewerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Status_ExpiresAt",
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "Status", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "reviews");
        }
    }
}
