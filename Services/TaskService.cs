using CrmApp.Data;
using CrmApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmApp.Services;

public class TaskService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public TaskService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<TaskItem>> GetTasksAsync(TaskFilter? filter = null)
    {
        using var db = await _factory.CreateDbContextAsync();
        var query = db.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .Include(t => t.Project)
            .Include(t => t.SubTasks)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);
            if (filter.Priority.HasValue)
                query = query.Where(t => t.Priority == filter.Priority.Value);
            if (filter.CreatorId.HasValue)
                query = query.Where(t => t.CreatorId == filter.CreatorId.Value);
            if (filter.AssigneeId.HasValue)
                query = query.Where(t => t.AssigneeId == filter.AssigneeId.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.ToDate.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                query = query.Where(t => t.Title.Contains(filter.SearchText) || t.Description.Contains(filter.SearchText));
            if (filter.ProjectId.HasValue)
                query = query.Where(t => t.ProjectId == filter.ProjectId.Value);
            if (filter.ExcludeDone)
                query = query.Where(t => t.Status != Models.TaskStatus.Done);
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Tasks
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Project)
            .Include(t => t.ParentTask)
            .Include(t => t.SubTasks).ThenInclude(s => s.Assignee)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        using var db = await _factory.CreateDbContextAsync();
        task.CreatedAt = DateTime.Now;

        // Generate task number
        var maxId = await db.Tasks.AnyAsync() ? await db.Tasks.MaxAsync(t => t.Id) : 0;
        task.TaskNumber = $"T-{(maxId + 1).ToString("D4")}";

        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task UpdateTaskAsync(TaskItem task, int? changedByUserId = null)
    {
        using var db = await _factory.CreateDbContextAsync();
        var existing = await db.Tasks.FindAsync(task.Id);
        if (existing == null) return;

        var oldStatus = existing.Status;

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Status = task.Status;
        existing.Priority = task.Priority;
        existing.DueDate = task.DueDate;
        existing.AssigneeId = task.AssigneeId;
        existing.ProjectId = task.ProjectId;
        existing.Type = task.Type;
        existing.WaitingReason = task.WaitingReason;

        // Log status change
        if (oldStatus != task.Status && changedByUserId.HasValue)
        {
            db.TaskStatusLogs.Add(new TaskStatusLog
            {
                TaskId = task.Id,
                UserId = changedByUserId.Value,
                OldStatus = oldStatus,
                NewStatus = task.Status,
                ChangedAt = DateTime.Now
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<TaskStatusLog>> GetStatusLogsAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.TaskStatusLogs
            .Include(l => l.Task)
            .Include(l => l.User)
            .OrderByDescending(l => l.ChangedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task DeleteTaskAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        var task = await db.Tasks.FindAsync(id);
        if (task != null)
        {
            db.Tasks.Remove(task);
            await db.SaveChangesAsync();
        }
    }

    public async Task AddCommentAsync(int taskId, int userId, string content)
    {
        using var db = await _factory.CreateDbContextAsync();
        var comment = new TaskComment
        {
            TaskId = taskId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.Now
        };
        db.TaskComments.Add(comment);
        await db.SaveChangesAsync();
    }

    public async Task MarkAsDoneAsync(int taskId, int? changedByUserId = null)
    {
        using var db = await _factory.CreateDbContextAsync();
        var task = await db.Tasks.FindAsync(taskId);
        if (task != null)
        {
            var oldStatus = task.Status;
            task.Status = Models.TaskStatus.Done;

            if (changedByUserId.HasValue)
            {
                db.TaskStatusLogs.Add(new TaskStatusLog
                {
                    TaskId = taskId,
                    UserId = changedByUserId.Value,
                    OldStatus = oldStatus,
                    NewStatus = Models.TaskStatus.Done,
                    ChangedAt = DateTime.Now
                });
            }

            await db.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<Models.TaskStatus, int>> GetStatusCountsAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Tasks
            .GroupBy(t => t.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task LinkSubTaskAsync(int parentTaskId, int childTaskId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var child = await db.Tasks.FindAsync(childTaskId);
        if (child == null || child.Id == parentTaskId) return;
        // Prevent circular reference
        if (child.SubTasks?.Any(s => s.Id == parentTaskId) == true) return;
        child.ParentTaskId = parentTaskId;
        await db.SaveChangesAsync();
    }

    public async Task UnlinkSubTaskAsync(int childTaskId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var child = await db.Tasks.FindAsync(childTaskId);
        if (child == null) return;
        child.ParentTaskId = null;
        await db.SaveChangesAsync();
    }

    public async Task<List<TaskItem>> GetAvailableForLinkingAsync(int parentTaskId)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Project)
            .Where(t => t.Id != parentTaskId
                && t.ParentTaskId == null
                && t.Status != Models.TaskStatus.Done
                && !db.Tasks.Any(p => p.Id == parentTaskId && p.ParentTaskId == t.Id))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}

public class TaskFilter
{
    public Models.TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public int? CreatorId { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchText { get; set; }
    public int? ProjectId { get; set; }
    public bool ExcludeDone { get; set; }
}
