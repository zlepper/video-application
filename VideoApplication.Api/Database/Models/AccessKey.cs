using NodaTime;

namespace VideoApplication.Api.Database.Models;

public class AccessKey
{
    public Guid Id { get; set; }

    // Hashed
    public string Value { get; set; } = null!; 
    
    public Instant LoginDate { get; set; }
    
    public User User { get; set; } = null!;
    public Guid UserId { get; set; }
    
}