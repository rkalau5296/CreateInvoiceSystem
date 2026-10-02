namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;

public record LoginUserDto(string Email, string Password, bool RememberMe);
