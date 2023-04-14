using System.ComponentModel.DataAnnotations;

namespace Netial.Database.Models;

public class Comment {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    
    // Навигационные свойства
    public virtual User Author { get; set; }
    public virtual Post Post { get; set; }
    public virtual ICollection<User> LikedBy { get; set; }
}