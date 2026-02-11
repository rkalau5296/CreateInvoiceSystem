namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;

public record ChangePasswordDto(string OldPassword, string NewPassword, string ConfirmPassword);
