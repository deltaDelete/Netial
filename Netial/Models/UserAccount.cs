using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Netial.Models; 

public class UserAccount {
    [Column("account_id")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [ForeignKey("user_id")]
    public Guid UserId { get; set; }
    public User User { get; set; }
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