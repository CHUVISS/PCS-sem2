namespace BlazorClient.Services;

public class ThemeService
{
    public bool IsDark { get; private set; } = false;

    public event Action? OnChange;

    public void Toggle()
    {
        IsDark = !IsDark;
        OnChange?.Invoke();
    }

    public string ThemeClass => IsDark ? "theme-dark" : "theme-light";
}
