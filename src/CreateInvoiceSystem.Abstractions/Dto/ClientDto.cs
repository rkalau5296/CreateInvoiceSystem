namespace CreateInvoiceSystem.Abstractions.Dto;

public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    int AddressId,
    AddressDto AddressDto,
    int UserId,
    UserDto UserDto    
);
