using Crm.Api.Security;

namespace Crm.Api.Dtos;

public record UserResponse
(
    int Id,
    string Email,
    UserRole Role,
    DateTimeOffset CreatedAt
);
