using CrmApp.Data;
using CrmApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Use PORT env variable from Cloud Run
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbUrl))
{
    // Convert postgresql:// URL to Npgsql connection string
    var uri = new Uri(dbUrl);
    var userInfo = uri.UserInfo.Split(':');
    var connStr = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseNpgsql(connStr));
}
else
{
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseSqlite("Data Source=crm.db"));
}

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
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

// Health check endpoint - keeps the server alive
app.MapGet("/health", () => Results.Ok("alive"));

app.MapRazorComponents<CrmApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
