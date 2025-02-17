namespace SocialMediaBackend.Models;

public class Post
{
    public int Id { get; set; }
    public int userId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime createdAt { get; set; } = DateTime.UtcNow;
}