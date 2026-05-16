using Api.Security;

namespace Api.Dtos;

public record UserResponse
(
    int Id,
    string Email,
    UserRole Role,
    DateTimeOffset CreatedAt
);
