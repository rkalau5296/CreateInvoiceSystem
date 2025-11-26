namespace CreateInvoiceSystem.Abstractions.Dto;

public record ClientDto(
    int ClientId,   
    string Name,
    string Nip,
    int AddressId,
    //int UserId,    
    AddressDto AddressDto
);
