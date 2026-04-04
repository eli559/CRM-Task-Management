using System.ComponentModel.DataAnnotations;

namespace CrmApp.Models;

public class TaskStatusLog
{
    public int Id { get; set; }

    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public TaskStatus OldStatus { get; set; }
    public TaskStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.Now;
}
