using Microsoft.AspNetCore.Identity;

namespace VideoApplication.Api.Database.Models;

public class Role : IdentityRole<Guid>
{
    public Role() { }
    public Role(string name) : base(name) {}
}