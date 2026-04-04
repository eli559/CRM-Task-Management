using Microsoft.EntityFrameworkCore;
using CrmApp.Models;

namespace CrmApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskStatusLog> TaskStatusLogs => Set<TaskStatusLog>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<TaskLabel> TaskLabels => Set<TaskLabel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<TaskLabel>(entity =>
        {
            entity.HasKey(tl => new { tl.TaskId, tl.LabelId });
            entity.HasOne(tl => tl.Task).WithMany(t => t.TaskLabels).HasForeignKey(tl => tl.TaskId);
            entity.HasOne(tl => tl.Label).WithMany(l => l.TaskLabels).HasForeignKey(tl => tl.LabelId);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed admin user (password: eliezer)
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "eliezer",
            PasswordHash = BCryptHelper.HashPassword("eliezer"),
            FullName = "אליעזר כהן",
            Email = "eliezer@crm.local",
            Role = UserRole.Admin,
            IsActive = true,
            IsApproved = true,
            CreatedAt = new DateTime(2024, 1, 1)
        });
    }
}

// Simple BCrypt-like hash using HMACSHA256 for simplicity (no extra NuGet needed)
public static class BCryptHelper
{
    private const string Salt = "CrmApp2024SecretSalt!";

    public static string HashPassword(string password)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(Salt));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
