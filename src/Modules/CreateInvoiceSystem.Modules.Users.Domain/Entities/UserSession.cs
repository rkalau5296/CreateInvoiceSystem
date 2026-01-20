namespace CreateInvoiceSystem.Modules.Users.Domain.Entities;

public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public Guid RefreshToken { get; set; }
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
}
