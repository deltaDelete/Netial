using System.ComponentModel.DataAnnotations;

namespace Netial.Database.Models;

public class Group {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public GroupPermissions Permissions { get; set; }
    public string Name { get; set; }

    public ICollection<User> Users { get; set; }
}