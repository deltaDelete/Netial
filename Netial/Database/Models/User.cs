using System.ComponentModel.DataAnnotations;

namespace Netial.Database.Models;

public class User {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public int Rating { get; set; }
    
    // доп данные для входа
    
    [EmailAddress]
    public string Email { get; set; }
    [EmailAddress]
    public string EmailNormalized { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    
    public bool IsEmailConfirmed { get; set; }
    
    // Навигационные свойства
    public virtual ICollection<Group> Groups { get; set; }
    public virtual ICollection<Post> UpvotedPosts { get; set; }
    public virtual ICollection<Post> DownvotedPosts { get; set; }
    public virtual ICollection<Post> ViewedPosts { get; set; }
    public virtual ICollection<Post> Posts { get; set; }
    public virtual ICollection<Comment> LikedComments { get; set; }
    public virtual ICollection<Comment> Comments { get; set; }
}