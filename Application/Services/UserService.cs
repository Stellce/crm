using Application.DTOs;
using Domain.Entities;
using Application.Exceptions;
using Domain.Security;
using Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserService(
    IAppDbContext context,
    IPasswordHasher<User> hasher
)
{
    public async Task<List<UserResponse>> GetUsers(int currentUserId, CancellationToken cancellationToken)
    {
        var currentUserRole = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == currentUserId)
            .Select(u => (UserRole?) u.Role)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppException(ErrorCode.UserNotFound);

        IQueryable<User> query = context.Users.AsNoTracking();

        query = currentUserRole switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(user => user.Role != UserRole.SuperAdmin),
            _ => throw new AppException(ErrorCode.Forbidden)
        };

        return await query
            .Select(user => new UserResponse
            (
                user.Id,
                user.Email,
                user.Role,
                user.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserResponse> GetUserById(int targetUserId, int currentUserId, CancellationToken cancellationToken)
    {
        var currentUserRole = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == currentUserId)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new AppException(ErrorCode.UserNotFound);

        var targetUser = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == targetUserId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new AppException(ErrorCode.UserNotFound);

        if (!CanViewUser(currentUserRole, targetUser.Role))
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

    public async Task<UserResponse> CreateManager(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new AppException(ErrorCode.UserAlreadyExists);
        }

        var user = new User
        {
            Email = request.Email,
            Role = UserRole.Manager,
            CreatedAt = DateTimeOffset.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return new UserResponse
        (
            user.Id,
            user.Email,
            user.Role,
            user.CreatedAt
        );
    }

    public async Task<UserResponse> CreateAdmin(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new AppException(ErrorCode.UserAlreadyExists);
        }

        var user = new User
        {
            Email = request.Email,
            Role = UserRole.Admin,
            CreatedAt = DateTimeOffset.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return new UserResponse
        (
            user.Id,
            user.Email,
            user.Role,
            user.CreatedAt
        );
    }

    public async Task DeleteUser(int targetUserId, int currentUserId, CancellationToken cancellationToken)
    {
        var currentUser = await context.Users.FindAsync([currentUserId], cancellationToken)
            ?? throw new AppException(ErrorCode.UserNotFound);
        var targetUser = await context.Users.FindAsync([targetUserId], cancellationToken)
            ?? throw new AppException(ErrorCode.UserNotFound);

        if (!CanViewUser(currentUser.Role, targetUser.Role))
        {
            throw new AppException(ErrorCode.Forbidden);
        }

        context.Users.Remove(targetUser);
        await context.SaveChangesAsync(cancellationToken);
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