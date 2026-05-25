using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.UserService.Infrastructure.Persistence;
using TrustMarket.UserService.IntegrationTests.Factories;
using Xunit;

namespace TrustMarket.UserService.IntegrationTests.Integration;

[Collection("UserService")]
public class RegisterIntegrationTests : IClassFixture<UserServiceWebAppFactory>, IAsyncLifetime
{
    private readonly UserServiceWebAppFactory _factory;
    private readonly HttpClient _client;

    public RegisterIntegrationTests(PostgresContainerFixture postgres)
    {
        _factory = new UserServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ValidRequest_SavesUserToDb()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Анна",
            lastName = "Коваль",
            email = "anna@example.com",
            password = "Password123!",
            passwordConfirm = "Password123!",
            agreeToTerms = true,
            wantsNewsletter = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "anna@example.com");

        user.Should().NotBeNull();
        user!.FirstName.Should().Be("Анна");
        user.PasswordHash.Should().NotBe("Password123!");
    }

    [Fact]
    public async Task Register_ValidRequest_SendsConfirmationEmail()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Іван",
            lastName = "Петров",
            email = "ivan@example.com",
            password = "Password123!",
            passwordConfirm = "Password123!",
            agreeToTerms = true,
            wantsNewsletter = false
        });

        _factory.EmailSender.ConfirmationEmails
            .Should().ContainSingle(e => e.To == "ivan@example.com");
        _factory.EmailSender.ConfirmationEmails[0].Url
            .Should().Contain("verify-email");
    }

    [Fact]
    public async Task ConfirmEmail_ValidToken_SetsEmailConfirmedInDb()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Марія",
            lastName = "Бойко",
            email = "maria@example.com",
            password = "Password123!",
            passwordConfirm = "Password123!",
            agreeToTerms = true,
            wantsNewsletter = false
        });

        var confirmUrl = _factory.EmailSender.ConfirmationEmails[0].Url;
        var token = new Uri(confirmUrl).Query.TrimStart('?').Split('=')[1];

        var confirmResponse = await _client.GetAsync($"/api/auth/verify-email?token={token}");
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "maria@example.com");

        user!.IsEmailConfirmed.Should().BeTrue();
    }
}
