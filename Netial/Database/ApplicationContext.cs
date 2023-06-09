﻿using Microsoft.EntityFrameworkCore;
using Netial.Database.Models;

namespace Netial.Database;

public class ApplicationContext : DbContext {
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationContext> _logger;
    private string _connectionString = string.Empty;

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Size> Sizes { get; set; } = null!;
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    public ApplicationContext(DbContextOptions<ApplicationContext> options, IConfiguration configuration, ILogger<ApplicationContext> logger) : this(options) {
        _configuration = configuration;
        _logger = logger;
        _connectionString = _configuration.GetConnectionString("PSQL");
        Database.EnsureCreated();
        //_logger.LogInformation(Database.GenerateCreateScript());
    }

    private ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseLazyLoadingProxies();
        optionsBuilder.UseNpgsql(_connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        #region one2many

        // У комментариев 1 автор
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Comments)
            .WithOne(c => c.Author);
        // У постов 1 автор
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Posts)
            .WithOne(p => p.Author);
        // Комментарий может быть расположен только под одним постом
        modelBuilder
            .Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post);
        // Сообщение только в одном чате
        modelBuilder
            .Entity<Chat>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Chat);
        // У чата только один владелец, у владельца несколько чатов
        modelBuilder
            .Entity<Chat>()
            .HasOne(c => c.Owner)
            .WithMany(u => u.OwnedChats);
        // У сообщения только один автор, у автора несколько сообщений
        modelBuilder
            .Entity<Message>()
            .HasOne(m => m.Author)
            .WithMany(u => u.Messages);

        #endregion

        #region many2many

        // Пользователь может лайкнуть несколько комментариев
        // Комментарии могут быть лайкнуты несколькими пользователями
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.LikedComments)
            .WithMany(c => c.LikedBy)
            .UsingEntity("comment_likes");
        // Пользователь может просмотреть несколько постов
        // Посты могут быть просмотрены несколькими пользователями
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.ViewedPosts)
            .WithMany(p => p.ViewedBy)
            .UsingEntity("post_views");
        // Пользователь может апвоутнуть несколько постов
        // Посты могут быть апвоутнуты несколькими пользователями
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.UpvotedPosts)
            .WithMany(p => p.UpvotedBy)
            .UsingEntity("post_upvotes");
        // Пользователь может даунвоутнуть несколько постов
        // Посты могут быть даунвоутнуты несколькими пользователями
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.DownvotedPosts)
            .WithMany(p => p.DownvotedBy)
            .UsingEntity("post_downvotes");
        // У пользователя несколько групп у группы несколько пользователей
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Groups)
            .WithMany(g => g.Users);

        // У поста несколько вложений и вложение может быть в нескольких постах
        modelBuilder
            .Entity<Post>()
            .HasMany(p => p.Attachments)
            .WithMany(a => a.Posts)
            .UsingEntity("post_attachments");
        // В чате несколько членов, у каждого члена несколько чатов
        modelBuilder
            .Entity<Chat>()
            .HasMany(c => c.Members)
            .WithMany(u => u.Chats)
            .UsingEntity("chat_members");

        #endregion

        #region defaults

        modelBuilder
            .Entity<Post>()
            .Property(p => p.CreationDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        modelBuilder
            .Entity<Comment>()
            .Property(c => c.CreationDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        modelBuilder
            .Entity<User>()
            .Property(u => u.CreationDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        modelBuilder
            .Entity<Chat>()
            .Property(c => c.CreationDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        modelBuilder
            .Entity<Message>()
            .Property(c => c.CreationDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        

        #endregion
        
        base.OnModelCreating(modelBuilder);
    }
}
