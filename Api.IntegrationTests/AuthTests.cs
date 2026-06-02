using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Api.IntegrationTests;

public class AuthTests(
    SqlServerFixture sqlServer,
    RedisFixture redis) 
    : IntegrationTestBase(sqlServer, redis), 
        IClassFixture<SqlServerFixture>,
        IClassFixture<RedisFixture>
{

    [Fact]
    public async Task Login_WithSeededSuperAdmin_ReturnsTokens()
    {
        var request = new LoginRequest(
            "superadmin@crm.local",
            "SuperAdmin123!"
        );

        var response = await Client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        auth.Should().NotBeNull();
        auth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest(
            "superadmin@crm.local",
            "wrongpassword"
        );

        var response = await Client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WhenRateLimitExceeded_ReturnsTooManyRequests()
    {
        var request = new LoginRequest(
            "superadmin@crm.local",
            "wrongpassword"
        );

        HttpResponseMessage response = null!;

        for (var i = 0; i < 6; i++)
        {
            response = await Client.PostAsJsonAsync("/api/auth/login", request);
        }

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}