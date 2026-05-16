namespace Api.Dtos;

public record PatchCustomerRequest(
    string? Name,
    string? Email
)
{ }