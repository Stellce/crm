using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public record CreateCustomerRequest(
    [Required]
    string Name,
    [EmailAddress]
    string Email
);