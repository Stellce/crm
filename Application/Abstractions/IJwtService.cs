using Domain.Entities;

namespace Application.Abstractions;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string token);
}