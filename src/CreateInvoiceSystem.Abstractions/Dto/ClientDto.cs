namespace CreateInvoiceSystem.Abstractions.DTO;

public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    int AddressId,
    //int UserId,    
    AddressDto AddressDto
);
