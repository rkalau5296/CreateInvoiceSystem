namespace CreateInvoiceSystem.Abstractions.EntitiesBases
{
    public abstract class AddressBase
    {
        public int AddressId { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
    }
}
