namespace Netial.Database.Models;

public class Chat {
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;

    // Навигационные свойства
    public virtual User Owner { get; set; }
    public virtual ICollection<User> Members { get; set; }
    public virtual ICollection<Message> Messages { get; set; }
}