using System.ComponentModel.DataAnnotations.Schema;

namespace Netial.Models;

public class User {
    [Column("user_id")]
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
}
