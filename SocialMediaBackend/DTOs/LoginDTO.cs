namespace SocialMediaBackend.DTOs;

public class LoginDTO
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
