using System.ComponentModel.DataAnnotations;

namespace SocialMediaBackend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        [Required]
        public string Role { get; set; } = "User";

        public bool IsEmailVerified { get; set; } = false; 
        public string? EmailVerificationToken { get; set; }

        public bool IsMfaEnabled { get; set; } = false;
        public string? MfaSecret { get; set; }
        public string? RecoveryCodes { get; set; }
    }
}