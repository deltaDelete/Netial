using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Netial.Models;

public class Attachment {
    [Key]
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public string Link {
        get => $"/images/attachments/{Id}";
    }
    public ICollection<Size> Sizes { get; set; }
    public ICollection<Post> Posts { get; set; }
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