using System.ComponentModel.DataAnnotations;

namespace CrmApp.Models;

public class TaskComment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "תוכן התגובה הוא שדה חובה")]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
