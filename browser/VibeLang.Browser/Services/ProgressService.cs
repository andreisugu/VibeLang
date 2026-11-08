using System.Text.Json;
using Microsoft.JSInterop;

namespace VibeLang.Browser.Services;

/// <summary>Reads the demo progress that lesson-parser.js stores in localStorage.</summary>
public class ProgressService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IJSRuntime _js;

    public ProgressService(IJSRuntime js) => _js = js;

    public async Task<ProgressState> GetAsync()
    {
        var raw = await _js.InvokeAsync<string?>("localStorage.getItem", "vibelang-demo-progress");
        if (string.IsNullOrWhiteSpace(raw)) return new ProgressState();
        try
        {
            return JsonSerializer.Deserialize<ProgressState>(raw, JsonOptions) ?? new ProgressState();
        }
        catch
        {
            return new ProgressState();
        }
    }

    public async Task ResetAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "vibelang-demo-progress");
    }
}

public class ProgressState
{
    public List<int> Lessons { get; set; } = new();
    public int Xp { get; set; }
    public int? LastLessonId { get; set; }
}
