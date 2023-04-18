using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Netial.Database.Models;

public class Post {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Text { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;

    public int Upvotes { get; set; }

    public int Downvotes { get; set; }

    public int Views { get; set; }
    public int Shares { get; set; }

    public int CommentCount { get; set; }

    // Навигационные свойства
    public virtual User Author { get; set; }
    public virtual ICollection<Attachment> Attachments { get; set; }
    public virtual ICollection<Comment> Comments { get; set; }
    public virtual ICollection<User> UpvotedBy { get; set; }
    public virtual ICollection<User> DownvotedBy { get; set; }
    public virtual ICollection<User> ViewedBy { get; set; }
}