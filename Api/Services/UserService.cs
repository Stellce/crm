using Api.Data;
using Api.Dtos;
using Api.Entities;
using Api.Exceptions;
using Api.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class UserService(
    AppDbContext context,
    IPasswordHasher<User> hasher
)
{
    public Task<List<UserResponse>> GetUsers(int currentUserId)
    {
        var currentUser = context.Users.Find(currentUserId) ?? throw new AppException(ErrorCode.UserNotFound);

        IQueryable<User> query = context.Users;

        query = currentUser.Role switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(user => user.Role != UserRole.SuperAdmin),
            _ => throw new AppException(ErrorCode.Forbidden)
        };

        return query
            .Select(user => new UserResponse
            (
                user.Id,
                user.Email,
                user.Role,
                user.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<UserResponse> GetUserById(int targetUserId, int currentUserId)
    {
        var currentUser = await context.Users.FindAsync(currentUserId) ?? throw new AppException(ErrorCode.UserNotFound);
        var targetUser = await context.Users.FindAsync(targetUserId) ?? throw new AppException(ErrorCode.UserNotFound);

        if (!CanViewUser(currentUser.Role, targetUser.Role))
        {
            throw new AppException(ErrorCode.Forbidden);
        }

        return new UserResponse
        (
            targetUser.Id,
            targetUser.Email,
            targetUser.Role,
            targetUser.CreatedAt
        );
    }

    public async Task<UserResponse> CreateManager(CreateUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            Role = UserRole.Manager,
            CreatedAt = DateTimeOffset.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserResponse
        (
            user.Id,
            user.Email,
            user.Role,
            user.CreatedAt
        );
    }

    public async Task<UserResponse> CreateAdmin(CreateUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            Role = UserRole.Admin,
            CreatedAt = DateTimeOffset.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserResponse
        (
            user.Id,
            user.Email,
            user.Role,
            user.CreatedAt
        );
    }

    public async Task DeleteUser(int targetUserId, int currentUserId)
    {
        var currentUser = await context.Users.FindAsync(currentUserId) ?? throw new AppException(ErrorCode.UserNotFound);
        var targetUser = await context.Users.FindAsync(targetUserId) ?? throw new AppException(ErrorCode.UserNotFound);

        if (!CanViewUser(currentUser.Role, targetUser.Role))
        {
            throw new AppException(ErrorCode.Forbidden);
        }

        context.Users.Remove(targetUser);
        await context.SaveChangesAsync();
    }

    private static bool CanViewUser(UserRole currentUserRole, UserRole targetUserRole)
    {
        return currentUserRole switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => targetUserRole != UserRole.SuperAdmin,
            _ => false
        };
    }
}