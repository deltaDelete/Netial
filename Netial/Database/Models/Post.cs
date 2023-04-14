using System.ComponentModel.DataAnnotations;

namespace Netial.Database.Models; 

public class Post {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Text { get; set; }

    public int Upvotes {
        get => UpvotedBy.Count();
    }

    public int Downvotes {
        get => DownvotedBy.Count();
    }

    public int Views {
        get => ViewedBy.Count();
    }
    public int Shares { get; set; }
    public User Author { get; set; }
    public ICollection<Attachment> Attachments { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<User> UpvotedBy { get; set; }
    public ICollection<User> DownvotedBy { get; set; }
    public ICollection<User> ViewedBy { get; set; }
}