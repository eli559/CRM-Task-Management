using CrmApp.Data;
using CrmApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=crm.db"));

builder.Services.AddSingleton<RealtimeService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<LabelService>();
builder.Services.AddScoped<AppState>();

var app = builder.Build();

// Auto-migrate and seed
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.EnsureCreated();

    // Safe schema upgrades - add missing columns without losing data
    var conn = db.Database.GetDbConnection();
    conn.Open();
    using var cmd = conn.CreateCommand();

    // Check and add WaitingReason column if missing
    cmd.CommandText = "PRAGMA table_info('Tasks')";
    var columns = new List<string>();
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read()) columns.Add(reader.GetString(1));
    }

    if (!columns.Contains("Type"))
    {
        cmd.CommandText = "ALTER TABLE Tasks ADD COLUMN Type INTEGER NOT NULL DEFAULT 0";
        cmd.ExecuteNonQuery();
    }

    if (!columns.Contains("ParentTaskId"))
    {
        cmd.CommandText = "ALTER TABLE Tasks ADD COLUMN ParentTaskId INTEGER NULL REFERENCES Tasks(Id)";
        cmd.ExecuteNonQuery();
    }

    if (!columns.Contains("TaskNumber"))
    {
        cmd.CommandText = "ALTER TABLE Tasks ADD COLUMN TaskNumber TEXT NULL";
        cmd.ExecuteNonQuery();
        // Backfill existing tasks
        cmd.CommandText = "UPDATE Tasks SET TaskNumber = 'T-' || printf('%04d', Id) WHERE TaskNumber IS NULL";
        cmd.ExecuteNonQuery();
    }

    if (!columns.Contains("WaitingReason"))
    {
        cmd.CommandText = "ALTER TABLE Tasks ADD COLUMN WaitingReason TEXT NULL";
        cmd.ExecuteNonQuery();
    }

    // Check and add IsApproved column to Users if missing
    cmd.CommandText = "PRAGMA table_info('Users')";
    var userColumns = new List<string>();
    using (var reader2 = cmd.ExecuteReader())
    {
        while (reader2.Read()) userColumns.Add(reader2.GetString(1));
    }

    if (!userColumns.Contains("IsApproved"))
    {
        cmd.CommandText = "ALTER TABLE Users ADD COLUMN IsApproved INTEGER NOT NULL DEFAULT 1";
        cmd.ExecuteNonQuery();
    }

    // Create Projects table if missing
    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Projects'";
    var projectsExist = cmd.ExecuteScalar() != null;
    if (!projectsExist)
    {
        cmd.CommandText = @"CREATE TABLE Projects (
            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            CreatedAt TEXT NOT NULL DEFAULT '2024-01-01')";
        cmd.ExecuteNonQuery();
    }

    // Add ProjectId to Tasks if missing
    if (!columns.Contains("ProjectId"))
    {
        cmd.CommandText = "ALTER TABLE Tasks ADD COLUMN ProjectId INTEGER NULL REFERENCES Projects(Id)";
        cmd.ExecuteNonQuery();
    }

    // Create Labels table if missing
    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Labels'";
    var labelsExist = cmd.ExecuteScalar() != null;
    if (!labelsExist)
    {
        cmd.CommandText = @"CREATE TABLE Labels (
            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Color TEXT NOT NULL DEFAULT '#58a6ff')";
        cmd.ExecuteNonQuery();
    }

    // Create TaskLabels table if missing
    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TaskLabels'";
    var taskLabelsExist = cmd.ExecuteScalar() != null;
    if (!taskLabelsExist)
    {
        cmd.CommandText = @"CREATE TABLE TaskLabels (
            TaskId INTEGER NOT NULL,
            LabelId INTEGER NOT NULL,
            PRIMARY KEY (TaskId, LabelId),
            FOREIGN KEY (TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE,
            FOREIGN KEY (LabelId) REFERENCES Labels(Id) ON DELETE CASCADE)";
        cmd.ExecuteNonQuery();
    }

    // Create TaskStatusLogs table if missing
    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TaskStatusLogs'";
    var logsExist = cmd.ExecuteScalar() != null;
    if (!logsExist)
    {
        cmd.CommandText = @"CREATE TABLE TaskStatusLogs (
            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            TaskId INTEGER NOT NULL,
            UserId INTEGER NOT NULL,
            OldStatus INTEGER NOT NULL,
            NewStatus INTEGER NOT NULL,
            ChangedAt TEXT NOT NULL,
            FOREIGN KEY (TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE,
            FOREIGN KEY (UserId) REFERENCES Users(Id))";
        cmd.ExecuteNonQuery();
    }

    conn.Close();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<CrmApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
