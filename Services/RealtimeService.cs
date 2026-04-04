namespace CrmApp.Services;

public class RealtimeService
{
    public event Action? OnDataChanged;

    public void NotifyAll()
    {
        OnDataChanged?.Invoke();
    }
}
