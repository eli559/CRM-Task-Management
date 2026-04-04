using System.ComponentModel.DataAnnotations;

namespace CrmApp.Models;

public enum TaskStatus
{
    New,
    InProgress,
    Waiting,
    Done
}

public enum TaskType
{
    Task,
    Bug,
    Feature,
    Improvement,
    Urgent
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class TaskItem
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string TaskNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "כותרת היא שדה חובה")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public TaskStatus Status { get; set; } = TaskStatus.New;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public TaskType Type { get; set; } = TaskType.Task;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? DueDate { get; set; }

    public int CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }
    public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();

    [MaxLength(500)]
    public string? WaitingReason { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}

public static class TaskStatusHelper
{
    public static string ToHebrew(this TaskStatus status) => status switch
    {
        TaskStatus.New => "חדש",
        TaskStatus.InProgress => "בטיפול",
        TaskStatus.Waiting => "ממתין",
        TaskStatus.Done => "הושלם",
        _ => status.ToString()
    };

    public static string ToBadgeClass(this TaskStatus status) => status switch
    {
        TaskStatus.New => "badge-new",
        TaskStatus.InProgress => "badge-progress",
        TaskStatus.Waiting => "badge-waiting",
        TaskStatus.Done => "badge-done",
        _ => "badge-new"
    };
}

public static class TaskTypeHelper
{
    public static string ToHebrew(this TaskType type) => type switch
    {
        TaskType.Task => "משימה",
        TaskType.Bug => "באג",
        TaskType.Feature => "פיצ'ר",
        TaskType.Improvement => "שיפור",
        TaskType.Urgent => "דחוף",
        _ => type.ToString()
    };

    public static string ToBadgeClass(this TaskType type) => type switch
    {
        TaskType.Task => "type-task",
        TaskType.Bug => "type-bug",
        TaskType.Feature => "type-feature",
        TaskType.Improvement => "type-improvement",
        TaskType.Urgent => "type-urgent",
        _ => "type-task"
    };

    public static string ToIconClass(this TaskType type) => type switch
    {
        TaskType.Task => "icon-tasks",
        TaskType.Bug => "icon-x",
        TaskType.Feature => "icon-plus",
        TaskType.Improvement => "icon-trending-up",
        TaskType.Urgent => "icon-clock",
        _ => "icon-tasks"
    };
}

public static class TaskPriorityHelper
{
    public static string ToHebrew(this TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "נמוכה",
        TaskPriority.Medium => "בינונית",
        TaskPriority.High => "גבוהה",
        TaskPriority.Critical => "קריטית",
        _ => priority.ToString()
    };

    public static string ToBadgeClass(this TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "priority-low",
        TaskPriority.Medium => "priority-medium",
        TaskPriority.High => "priority-high",
        TaskPriority.Critical => "priority-critical",
        _ => "priority-medium"
    };
}
