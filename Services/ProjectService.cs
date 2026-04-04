using CrmApp.Data;
using CrmApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmApp.Services;

public class ProjectService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProjectService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<Project>> GetAllProjectsAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Projects.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Project> CreateProjectAsync(string name)
    {
        using var db = await _factory.CreateDbContextAsync();
        var project = new Project { Name = name };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project;
    }

    public async Task UpdateProjectAsync(int id, string name)
    {
        using var db = await _factory.CreateDbContextAsync();
        var project = await db.Projects.FindAsync(id);
        if (project != null)
        {
            project.Name = name;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteProjectAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        var project = await db.Projects.FindAsync(id);
        if (project != null)
        {
            project.IsActive = false;
            await db.SaveChangesAsync();
        }
    }
}
