using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.UserService.Application.Users.Commands.GoogleAuth;
using TrustMarket.UserService.Infrastructure.Persistence;
using TrustMarket.UserService.IntegrationTests.Factories;
using Xunit;

namespace TrustMarket.UserService.IntegrationTests.Functional;

[Collection("UserService")]
public class AuthFunctionalTests : IAsyncLifetime
{
    private readonly UserServiceWebAppFactory _factory;
    private readonly HttpClient _client;

    public AuthFunctionalTests(PostgresContainerFixture postgres)
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
    public async Task Register_DuplicateEmail_Returns409()
    {
        var body = new
        {
            firstName = "Тест",
            lastName = "Юзер",
            email = "dup@example.com",
            password = "Password123!",
            passwordConfirm = "Password123!",
            agreeToTerms = true,
            wantsNewsletter = false
        };

        await _client.PostAsJsonAsync("/api/auth/register", body);
        var second = await _client.PostAsJsonAsync("/api/auth/register", body);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns400()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Тест",
            lastName = "Юзер",
            email = "login_wrong@example.com",
            password = "Password123!",
            passwordConfirm = "Password123!",
            agreeToTerms = true,
            wantsNewsletter = false
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login_wrong@example.com",
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_UnconfirmedEmail_Returns403WithCode()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Тест",
            lastName = "Юзер",
            email = "unconfirmed@example.com",
            password = "Password123!",
            passwordConfirm = "Password123!",
            agreeToTerms = true,
            wantsNewsletter = false
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "unconfirmed@example.com",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("EMAIL_NOT_VERIFIED");
    }

    [Fact]
    public async Task GoogleAuth_NewUser_Returns200WithIsNewUserTrue()
    {
        _factory.GoogleValidator.NextPayload = new GooglePayload(
            "google_sub_123", "google_user@gmail.com", "Анна", "Коваль");

        var response = await _client.PostAsJsonAsync("/api/auth/google", new
        {
            idToken = "fake-google-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isNewUser").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Register_UnknownEmail_Login_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nobody@example.com",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
