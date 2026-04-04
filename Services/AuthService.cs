using CrmApp.Data;
using CrmApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmApp.Services;

public class AuthService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AuthService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        using var db = await _factory.CreateDbContextAsync();
        var hash = BCryptHelper.HashPassword(password);
        var users = await db.Users.Where(u => u.IsActive && u.IsApproved).ToListAsync();
        return users.FirstOrDefault(u =>
            u.Username.Trim() == username.Trim() && u.PasswordHash == hash);
    }

    public async Task<bool> IsUserPendingApproval(string username)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Users.AnyAsync(u => u.Username.Trim() == username.Trim() && !u.IsApproved);
    }

    public async Task<int> GetPendingApprovalCountAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Users.CountAsync(u => !u.IsApproved);
    }

    public async Task ApproveUserAsync(int userId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsApproved = true;
            await db.SaveChangesAsync();
        }
    }

    public async Task RejectUserAsync(int userId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(userId);
        if (user != null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
    }

    public async Task<(bool Success, string Error)> RegisterAsync(string username, string password, string fullName, string email)
    {
        using var db = await _factory.CreateDbContextAsync();

        if (await db.Users.AnyAsync(u => u.Username == username))
            return (false, "שם משתמש כבר קיים במערכת");

        var user = new User
        {
            Username = username,
            PasswordHash = BCryptHelper.HashPassword(password),
            FullName = fullName,
            Email = email,
            Role = UserRole.User
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Users.OrderBy(u => u.FullName).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Users.FindAsync(id);
    }

    public async Task UpdateUserAsync(User user)
    {
        using var db = await _factory.CreateDbContextAsync();
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ToggleUserActiveAsync(int userId)
    {
        using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(userId);
        if (user == null) return false;
        user.IsActive = !user.IsActive;
        await db.SaveChangesAsync();
        return true;
    }
}
