namespace VibeLang.Browser.Services;

/// <summary>Async-free demo session. No server, no real authentication.</summary>
public class DemoSession
{
    public string Name { get; private set; } = "Guest";
    public bool IsSignedIn { get; private set; }

    public event Action? Changed;

    public void SignIn(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Demo Student" : name.Trim();
        IsSignedIn = true;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        Name = "Guest";
        IsSignedIn = false;
        Changed?.Invoke();
    }
}
