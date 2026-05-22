using Domain.Security;

namespace Application.DTOs;

public record UserResponse
(
    int Id,
    string Email,
    UserRole Role,
    DateTimeOffset CreatedAt
);
