using Microsoft.EntityFrameworkCore;
using Netial.Models;

namespace Netial.Database;

public class ApplicationContext : DbContext {
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationContext> _logger;
    private string _connectionString = string.Empty;

    public DbSet<User> Users { get; set; } = null;
    public DbSet<Group> Groups { get; set; } = null;

    public ApplicationContext(IConfiguration configuration, ILogger<ApplicationContext> logger) {
        _configuration = configuration;
        _logger = logger;
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
        Database.EnsureCreated();
        _logger.LogInformation(Database.GenerateCreateScript());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseMySql(_connectionString, new MySqlServerVersion(new Version(8,0,32)));
    }
}
