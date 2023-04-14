using System.ComponentModel.DataAnnotations;

namespace Netial.Models;

public class Comment {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; }
    public User Author { get; set; }
    public Post Post { get; set; }
    public ICollection<User> LikedBy { get; set; }
}