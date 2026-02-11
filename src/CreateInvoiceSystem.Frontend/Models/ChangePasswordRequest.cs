namespace CreateInvoiceSystem.Frontend.Models;

public record ChangePasswordRequest(ChangePasswordDto Dto);

public record ChangePasswordDto(
    string OldPassword,
    string NewPassword,
    string ConfirmPassword);

public record ChangePasswordResponse(bool IsSuccess, string Message);