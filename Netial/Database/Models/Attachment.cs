using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Netial.Database.Models;

public class Attachment {
    [Key]
    [JsonInclude]
    public Guid Id { get; set; }
    [JsonInclude]
    public string? Description { get; set; }
    [JsonInclude]
    public string Link {
        get => $"/images/attachments/{Id}";
    }
    
    // Навигационные свойства
    
    [JsonIgnore]
    public virtual ICollection<Size> Sizes { get; set; }
    [JsonIgnore]
    public virtual ICollection<Post> Posts { get; set; }
}

public class Size {
    public Size() {
        Id = Guid.NewGuid();
        Value = string.Empty;
    }

    public Size(string value) {
        Id = Guid.NewGuid();
        Value = value;
    }

    public Size(Guid id, string value) {
        Id = id;
        Value = value;
    }
    
    public Guid Id { get; set; }
    public string Value { get; set; }
}