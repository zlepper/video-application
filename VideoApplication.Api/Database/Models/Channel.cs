using NodaTime;

namespace VideoApplication.Api.Database.Models;

public class Channel
{
    public Guid Id { get; set; }

    public string IdentifierName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;

    public string Description { get; set; } = null!;
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public bool MarkedForDeletion { get; set; } = true;
    
    public Instant CreatedAt { get; set; }

    public ICollection<Upload> Uploads { get; set; } = null!;
}