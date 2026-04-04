using System.ComponentModel.DataAnnotations;

namespace CrmApp.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
