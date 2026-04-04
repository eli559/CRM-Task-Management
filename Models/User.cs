using System.ComponentModel.DataAnnotations;

namespace CrmApp.Models;

public enum UserRole
{
    User,
    Admin
}

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "שם משתמש הוא שדה חובה")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "סיסמה היא שדה חובה")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required(ErrorMessage = "שם מלא הוא שדה חובה")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public bool IsApproved { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
