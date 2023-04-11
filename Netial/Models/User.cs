using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Netial.Models;

public class User {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public int Rating { get; set; }
    
    // доп данные для входа
    
    [EmailAddress]
    public string Email { get; set; }
    [EmailAddress]
    public string EmailNormalized { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    
    public bool IsEmailConfirmed { get; set; }
    
    public ICollection<Group> Groups { get; set; }
}