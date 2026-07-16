using System.Net.Http.Json;
using System.Text.Json;

namespace SMM.Desktop.Services.Mods;

public sealed class ModUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly ModService _modService;

    public ModUpdateService(
        HttpClient httpClient,
        ModService modService)
    {
        _httpClient = httpClient;
        _modService = modService;
    }

    public async Task<List<ModUpdateResult>> CheckUpdatesAsync()
    {
        var mods = _modService.GetMods();

        var request = new ModUpdateRequest
        {
            Mods = mods
                .Where(mod =>
                    !string.IsNullOrWhiteSpace(mod.UniqueId) &&
                    !string.IsNullOrWhiteSpace(mod.Version) &&
                    mod.UpdateKeys.Count > 0)
                .Select(mod => new ModUpdateRequestItem
                {
                    Id = mod.UniqueId,
                    UpdateKeys = mod.UpdateKeys,
                    InstalledVersion = mod.Version,
                    IsBroken = false
                })
                .ToList()
        };

        if (request.Mods.Count == 0)
            return [];

        using var response = await _httpClient.PostAsJsonAsync(
            "https://smapi.io/api/v4.0.0/mods",
            request,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<ModUpdateResult>>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? [];
    }
}