namespace Netial.Database.Models; 

public class Message {
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime CreationDate { get; set; }

    // Навигационные свойства
    public virtual User Author { get; set; }
    public virtual Chat Chat { get; set; }
}