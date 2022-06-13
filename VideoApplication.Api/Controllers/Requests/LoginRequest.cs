using System.ComponentModel.DataAnnotations;

namespace VideoApplication.Api.Controllers;

public class LoginRequest
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    [MinLength(1)]
    public string Password { get; set; } = null!;
}