using System.ComponentModel.DataAnnotations;

namespace VideoApplication.Api.Controllers;

public class SignupRequest
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [MinLength(2)]
    public string Name { get; set; } = null!;
}