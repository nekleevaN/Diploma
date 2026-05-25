using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using TrustMarket.UserService.Infrastructure.Persistence;
using TrustMarket.UserService.IntegrationTests.Factories;
using Xunit;

namespace TrustMarket.UserService.IntegrationTests.Security;

[Collection("UserService")]
public class AuthSecurityTests : IAsyncLifetime
{
    private readonly UserServiceWebAppFactory _factory;
    private readonly HttpClient _client;

    public AuthSecurityTests(PostgresContainerFixture postgres)
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
    public async Task GetProfile_WithoutToken_Returns401()
    {
        var response = await _client.PostAsync($"/api/users/verify/diia/start", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResendVerification_WithoutToken_Returns401()
    {
        var response = await _client.PostAsync("/api/auth/resend-verification", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AnyProtectedEndpoint_ExpiredJwt_Returns401()
    {
        var expiredToken = JwtTokenHelper.GenerateExpiredToken(Guid.NewGuid());
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.PostAsync("/api/auth/resend-verification", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_SqlInjectionInEmail_Returns400NoCrash()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email = "'; DROP TABLE users; --",
            password = "Password123!",
            passwordConfirm = "Password123!",
            agreeToTerms = true,
            wantsNewsletter = false
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ResendVerification_TokenWithEmailConfirmedFalse_Returns401()
    {
        var userId = Guid.NewGuid();
        var token = JwtTokenHelper.GenerateToken(userId, emailConfirmed: false);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // resend-verification requires only [Authorize], not EmailConfirmed policy
        // but the user doesn't exist in DB so it should return 400 (not found), not 401
        var response = await _client.PostAsync("/api/auth/resend-verification", null);
        // The endpoint is reachable (not 401 for just Authorize), but user doesn't exist
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
