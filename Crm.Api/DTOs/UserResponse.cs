using Crm.Api.Security;

namespace Crm.Api.Dtos;

public record UserResponse(
    string Email,
    UserRole Role,
    DateTimeOffset CreatedAt
);