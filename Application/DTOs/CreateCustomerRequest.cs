using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateCustomerRequest(
    [Required]
    string Name,
    [EmailAddress]
    string Email
);