using System.Text.Json.Serialization;

namespace Api.Dtos;

public record RefreshTokenRequest(
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken
);