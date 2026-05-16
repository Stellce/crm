using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public record CreateUserRequest(
    [Required]
    [EmailAddress]
    string Email,
    [MinLength(8)]
    string Password
);