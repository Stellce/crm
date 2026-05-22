using System.Text.Json.Serialization;

namespace Application.DTOs;

public record AuthResponse(
    [property: JsonPropertyName("access_token")]
    string AccessToken,

    [property: JsonPropertyName("refresh_token")]
    string RefreshToken
);
