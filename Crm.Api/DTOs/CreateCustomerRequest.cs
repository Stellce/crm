using System.ComponentModel.DataAnnotations;

namespace Crm.Api.Dtos;

public record CreateCustomerRequest(
    [Required]
    string Name,
    [EmailAddress]
    string Email
);