using CrmApp.Models;

namespace CrmApp.Services;

public class AppState
{
    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public event Action? OnChange;

    public void Login(User user)
    {
        CurrentUser = user;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        CurrentUser = null;
        OnChange?.Invoke();
    }

    public void NotifyStateChanged() => OnChange?.Invoke();

    // Toast notifications
    public event Action<string, string>? OnToast; // message, type (success/error/info)

    public void ShowToast(string message, string type = "success")
    {
        OnToast?.Invoke(message, type);
    }
}
