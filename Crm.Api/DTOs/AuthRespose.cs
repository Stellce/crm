using System.Text.Json.Serialization;

namespace Crm.Api.Dtos;

public record AuthResponse(
    [property: JsonPropertyName("access_token")]
    string AccessToken
// [property: JsonPropertyName("refresh_token")]
// string RefreshToken
);
