using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Netial.Database.Models;

public class Comment {
    [JsonInclude]
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonInclude]
    public string Text { get; set; }
    [JsonInclude]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    [JsonInclude]
    public int Likes { get; set; }

    // Навигационные свойства
    [JsonInclude]
    public virtual User Author { get; set; }
    [JsonIgnore]
    public virtual Post Post { get; set; }
    [JsonIgnore]
    public virtual ICollection<User> LikedBy { get; set; }
}