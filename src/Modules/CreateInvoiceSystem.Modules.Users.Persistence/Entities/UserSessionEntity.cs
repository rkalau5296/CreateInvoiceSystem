namespace CreateInvoiceSystem.Modules.Users.Persistence.Entities;

public class UserSessionEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public Guid RefreshToken { get; set; }
    public DateTime LastActivityAt { get; set; }
    public bool IsRevoked { get; set; }
    
}
