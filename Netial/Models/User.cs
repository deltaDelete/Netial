using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Netial.Models;

public class User {
    [Column("user_id")]
    [Key]
    public Guid Id { get; set; }
    [Column("firstname")]
    public string FirstName { get; set; }
    [Column("lastname")]
    public string LastName { get; set; }
    [Column("birchdate")]
    public DateTime BirthDate { get; set; }
    [Column("creationdate")]
    public DateTime CreationDate { get; set; }
    [Column("rating")]
    public int Rating { get; set; }
    
    // доп данные для входа
    
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; }
    [EmailAddress]
    [Column("email_normalized")]
    public string EmailNormalized { get; set; }
    [Column("password_hash")]
    public string PasswordHash { get; set; }
    [Column("password_salt")]
    public string PasswordSalt { get; set; }
}