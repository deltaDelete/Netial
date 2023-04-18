using System.ComponentModel.DataAnnotations;

namespace Netial.Database.Models;

public class Group {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public GroupPermissions Permissions { get; set; }
    public string Name { get; set; }

    // Навигационные свойства
    public virtual ICollection<User> Users { get; set; }
}