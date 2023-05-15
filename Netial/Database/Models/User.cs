using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Netial.Database.Models;

public class User {
    [JsonInclude]
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonInclude]
    public string FirstName { get; set; }
    [JsonInclude]
    public string LastName { get; set; }
    [JsonInclude]
    public DateTime BirthDate { get; set; }
    [JsonInclude]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    [JsonInclude]
    public int Rating { get; set; }
    
    // доп данные для входа
    
    [JsonInclude]
    [EmailAddress]
    public string Email { get; set; }
    [JsonInclude]
    [EmailAddress]
    public string EmailNormalized { get; set; }
    [JsonIgnore]
    public string PasswordHash { get; set; }
    [JsonIgnore]
    public string PasswordSalt { get; set; }
    
    public bool IsEmailConfirmed { get; set; }
    
    // Навигационные свойства
    [JsonIgnore]
    public virtual ICollection<Group> Groups { get; set; }
    [JsonIgnore]
    public virtual ICollection<Post> UpvotedPosts { get; set; }
    [JsonIgnore]
    public virtual ICollection<Post> DownvotedPosts { get; set; }
    [JsonIgnore]
    public virtual ICollection<Post> ViewedPosts { get; set; }
    [JsonIgnore]
    public virtual ICollection<Post> Posts { get; set; }
    [JsonIgnore]
    public virtual ICollection<Comment> LikedComments { get; set; }
    [JsonIgnore]
    public virtual ICollection<Comment> Comments { get; set; }
    [JsonIgnore]
    public virtual ICollection<Message> Messages { get; set; }
    [JsonIgnore]
    public virtual ICollection<Chat> Chats { get; set; }
    [JsonIgnore]
    public virtual ICollection<Chat> OwnedChats { get; set; }
}