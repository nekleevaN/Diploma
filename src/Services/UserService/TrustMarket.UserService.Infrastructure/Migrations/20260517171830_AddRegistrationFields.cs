using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustMarket.UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                schema: "users",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "users",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "AuthProvider",
                schema: "users",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                schema: "users",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationTokenExpiresAt",
                schema: "users",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "users",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "users",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Legacy");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                schema: "users",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "users",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNameChangedAt",
                schema: "users",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                schema: "users",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiresAt",
                schema: "users",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicNameMode",
                schema: "users",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                schema: "users",
                table: "Users",
                column: "Username",
                unique: true);

            // Legacy users: підтверджуємо пошту автоматично щоб не зламати існуючих юзерів
            migrationBuilder.Sql("""
                UPDATE users."Users"
                SET "IsEmailConfirmed" = true
                WHERE "IsEmailConfirmed" = false;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthProvider",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationTokenExpiresAt",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastNameChangedAt",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiresAt",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PublicNameMode",
                schema: "users",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                schema: "users",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "users",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
