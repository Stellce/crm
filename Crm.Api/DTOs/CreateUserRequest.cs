using System.ComponentModel.DataAnnotations;

namespace Crm.Api.Dtos;

public record CreateUserRequest(
    [Required]
    [EmailAddress]
    string Email,
    [MinLength(8)]
    string Password
);