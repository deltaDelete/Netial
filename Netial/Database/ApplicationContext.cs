using Microsoft.EntityFrameworkCore;
using Netial.Models;

namespace Netial.Database;

public class ApplicationContext : DbContext {
    private readonly IConfiguration _configuration;
    private string _connectionString = string.Empty;

    public DbSet<User> Users { get; set; } = null;
    public DbSet<UserAccount> Accounts { get; set; } = null;

    public ApplicationContext(IConfiguration configuration) {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseMySql(_connectionString, new MySqlServerVersion(new Version(8,0,32)));
    }
}
