namespace CreateInvoiceSystem.Abstractions.Dto;

public record UserDto(
    int UserId,
    string Name,
    string Email,
    string Password
 );
