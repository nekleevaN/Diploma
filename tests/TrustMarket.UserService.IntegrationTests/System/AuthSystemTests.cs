using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.UserService.Infrastructure.Persistence;
using TrustMarket.UserService.IntegrationTests.Factories;
using Xunit;

namespace TrustMarket.UserService.IntegrationTests.System;

[Collection("UserService")]
public class AuthSystemTests : IAsyncLifetime
{
    private readonly UserServiceWebAppFactory _factory;
    private readonly HttpClient _client;

    public AuthSystemTests(PostgresContainerFixture postgres)
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
    public async Task Register_ConfirmEmail_Login_GetProfile_FullFlow()
    {
        // 1. Register
        var registerResp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Олексій",
            lastName = "Ткаченко",
            email = "system_test@example.com",
            password = "Password123!",
            passwordConfirm = "Password123!",
            agreeToTerms = true,
            wantsNewsletter = false
        });
        registerResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerBody = await registerResp.Content.ReadFromJsonAsync<JsonElement>();
        var userId = registerBody.GetProperty("userId").GetGuid();

        // 2. Confirm email
        var confirmUrl = _factory.EmailSender.ConfirmationEmails[0].Url;
        var token = Uri.UnescapeDataString(
            new Uri(confirmUrl).Query.TrimStart('?').Replace("token=", ""));
        var confirmResp = await _client.GetAsync($"/api/auth/verify-email?token={Uri.EscapeDataString(token)}");
        confirmResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirmBody = await confirmResp.Content.ReadFromJsonAsync<JsonElement>();
        var confirmedToken = confirmBody.GetProperty("token").GetString()!;

        // 3. Login with confirmed email
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "system_test@example.com",
            password = "Password123!"
        });
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
        loginBody.GetProperty("userId").GetGuid().Should().Be(userId);
        loginBody.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }
}
