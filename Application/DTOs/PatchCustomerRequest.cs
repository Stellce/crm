namespace Application.DTOs;

public record PatchCustomerRequest(
    string? Name,
    string? Email
)
{ }