using Microsoft.JSInterop;

namespace VibeLang.Browser.Services;

/// <summary>Demo-only session: a name in localStorage. No server, no real auth.</summary>
public class DemoSession
{
    private const string Key = "vibelang-demo-name";
    private readonly IJSRuntime _js;

    public DemoSession(IJSRuntime js) => _js = js;

    public string Name { get; private set; } = "Guest";
    public bool IsSignedIn { get; private set; }

    public event Action? Changed;

    public async Task RestoreAsync()
    {
        var name = await _js.InvokeAsync<string?>("localStorage.getItem", Key);
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name!;
            IsSignedIn = true;
            Changed?.Invoke();
        }
    }

    public async Task SignInAsync(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Learner" : name.Trim();
        IsSignedIn = true;
        await _js.InvokeVoidAsync("localStorage.setItem", Key, Name);
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        Name = "Guest";
        IsSignedIn = false;
        await _js.InvokeVoidAsync("localStorage.removeItem", Key);
        Changed?.Invoke();
    }
}
