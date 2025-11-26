using System.ComponentModel.DataAnnotations;

namespace Monolith.Blazor.Services;

public interface ISignInManager
{
    Task<Result> SignInAsync(LoginModel model);

    Task SignOutAsync();
}

public sealed class LoginModel
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
