using CrmApp.Data;
using CrmApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmApp.Services;

public class LabelService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public LabelService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Label>> GetAllLabelsAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Labels.OrderBy(l => l.Name).ToListAsync();
    }

    public async Task<Label> CreateLabelAsync(string name, string color)
    {
        using var db = await _factory.CreateDbContextAsync();
        var label = new Label { Name = name, Color = color };
        db.Labels.Add(label);
        await db.SaveChangesAsync();
        return label;
    }

    public async Task UpdateLabelAsync(int id, string name, string color)
    {
        using var db = await _factory.CreateDbContextAsync();
        var label = await db.Labels.FindAsync(id);
        if (label != null)
        {
            label.Name = name;
            label.Color = color;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteLabelAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        var label = await db.Labels.FindAsync(id);
        if (label != null)
        {
            db.Labels.Remove(label);
            await db.SaveChangesAsync();
        }
    }

    public async Task SetTaskLabelsAsync(int taskId, List<int> labelIds)
    {
        using var db = await _factory.CreateDbContextAsync();
        var existing = await db.TaskLabels.Where(tl => tl.TaskId == taskId).ToListAsync();
        db.TaskLabels.RemoveRange(existing);

        foreach (var labelId in labelIds)
        {
            db.TaskLabels.Add(new TaskLabel { TaskId = taskId, LabelId = labelId });
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<int>> GetTaskLabelIdsAsync(int taskId)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.TaskLabels.Where(tl => tl.TaskId == taskId).Select(tl => tl.LabelId).ToListAsync();
    }
}
