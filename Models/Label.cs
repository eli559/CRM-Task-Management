using System.ComponentModel.DataAnnotations;

namespace CrmApp.Models;

public class Label
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(7)]
    public string Color { get; set; } = "#58a6ff";

    public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
}

public class TaskLabel
{
    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public int LabelId { get; set; }
    public Label Label { get; set; } = null!;
}
