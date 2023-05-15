using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Netial.Database.Models;

public class Post {
    [JsonInclude]
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonInclude]
    public string? Text { get; set; }
    [JsonInclude]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [JsonInclude]
    public int Upvotes { get; set; }

    [JsonInclude]
    public int Downvotes { get; set; }

    [JsonInclude]
    public int Views { get; set; }
    [JsonInclude]
    public int Shares { get; set; }

    [JsonInclude]
    public int CommentCount { get; set; }

    // Навигационные свойства
    [JsonInclude]
    public virtual User Author { get; set; }
    [JsonIgnore]
    public virtual ICollection<Attachment> Attachments { get; set; }
    [JsonIgnore]
    public virtual ICollection<Comment> Comments { get; set; }
    [JsonIgnore]
    public virtual ICollection<User> UpvotedBy { get; set; }
    [JsonIgnore]
    public virtual ICollection<User> DownvotedBy { get; set; }
    [JsonIgnore]
    public virtual ICollection<User> ViewedBy { get; set; }
}