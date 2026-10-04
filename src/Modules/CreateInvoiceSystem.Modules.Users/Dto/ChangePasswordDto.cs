namespace CreateInvoiceSystem.Modules.Users.Dto;

public record ChangePasswordDto(string OldPassword, string NewPassword, string ConfirmPassword);
