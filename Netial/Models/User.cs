using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Netial.Models;

public class User {
    [Column("user_id")]
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Column("firstname")]
    public string FirstName { get; set; }
    [Column("lastname")]
    public string LastName { get; set; }
    [Column("birchdate")]
    public DateTime BirthDate { get; set; }
    [Column("creationdate")]
    public DateTime CreationDate { get; set; } = DateTime.Now;
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
    
    [Column("is_email_confirmed")]
    public bool IsEmailConfirmed { get; set; }
    
    [ForeignKey("group")]
    public Group Group { get; set; }
}

[Flags]
public enum GroupPermissions {
    BanUser,
    BanPost,
    BanMessage,
    RemoveUser
}

public class Group {
    [Key]
    [Column("group_id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Column("permissions")]
    public GroupPermissions Permissions { get; set; }
    [Column("name")]
    public string Name { get; set; }
}