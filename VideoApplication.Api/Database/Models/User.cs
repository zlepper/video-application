using Microsoft.AspNetCore.Identity;

namespace VideoApplication.Api.Database.Models;

public class User : IdentityUser<Guid>
{
    public string Name { get; set; } = null!;

    public ICollection<AccessKey> AccessKeys { get; set; } = null!;
}