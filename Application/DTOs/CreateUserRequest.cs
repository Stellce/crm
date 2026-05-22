using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateUserRequest(
    [Required]
    [EmailAddress]
    string Email,
    [MinLength(8)]
    string Password
);