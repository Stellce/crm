using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record LoginRequest(
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    string Password
);