using System.Text.Json.Serialization;

namespace Application.DTOs;

public record RefreshTokenRequest(
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken
);