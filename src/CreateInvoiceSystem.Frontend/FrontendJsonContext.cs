using CreateInvoiceSystem.Frontend.Models;
using CreateInvoiceSystem.Frontend.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(AuthResponse))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(ForgotPasswordRequest))]
[JsonSerializable(typeof(ForgotPasswordResponse))]
[JsonSerializable(typeof(AuthService.ResetPasswordPayload))]
[JsonSerializable(typeof(ResetPasswordResponse))]
[JsonSerializable(typeof(RegisterUserRequest))]
[JsonSerializable(typeof(RegisterUserResponse))]
[JsonSerializable(typeof(ChangePasswordRequest))]
[JsonSerializable(typeof(ChangePasswordResponse))]
[JsonSerializable(typeof(ServerActivateResponse))]
[JsonSerializable(typeof(GetMeResponse))]
[JsonSerializable(typeof(GetUserResponse))]
[JsonSerializable(typeof(UpdateUserDto))]
[JsonSerializable(typeof(ClientDto))]
[JsonSerializable(typeof(ClientService.GetClientsResponse))]
[JsonSerializable(typeof(ClientService.UpdateClientPayload))]
[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(ProductService.GetProductsResponse))]
[JsonSerializable(typeof(ProductService.UpdateProductPayload))]
[JsonSerializable(typeof(InvoiceDto))]
[JsonSerializable(typeof(InvoiceService.GetInvoicesResponse))]
[JsonSerializable(typeof(InvoiceService.GetInvoiceResponse))]
[JsonSerializable(typeof(GetActualCurrencyRatesResponse))]
[JsonSerializable(typeof(GetSingleCurrencyRateResponse))]
[JsonSerializable(typeof(KsefService.KsefSettingsResponse))]
[JsonSerializable(typeof(KsefService.KsefTokenPayload))]
[JsonSerializable(typeof(KsefInvoicesResponse))]
[JsonSerializable(typeof(KsefInvoiceDetailsDto))]
[JsonSerializable(typeof(KsefStatusDto))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
internal partial class FrontendJsonContext : JsonSerializerContext;
