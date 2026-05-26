using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;

namespace Api.IntegrationTests;

public class AuthTests(SqlServerFixture sqlServer) : IClassFixture<SqlServerFixture>, IAsyncLifetime
{
    private readonly CrmApiFactory _factory = new(sqlServer.ConnectionString);
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Login_WithSeededSuperAdmin_ReturnsTokens()
    {
        var request = new LoginRequest(
            "superadmin@crm.local",
            "SuperAdmin123!"
        );

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

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

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}