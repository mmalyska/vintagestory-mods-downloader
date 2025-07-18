using System.Net.Http.Json;
using System.Text.Json;

namespace vintagestory_mods_downloader;

public class ModHttpClient : HttpClient
{
    JsonSerializerOptions SerializerSettings = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    
    public ModHttpClient()
    {
        BaseAddress = new Uri("https://mods.vintagestory.at/api/");
    }

    public async Task<ModDetails?> GetModDetails(string modId)
    {
        var response = await GetAsync($"mod/{modId}");
        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<ModEncapsulation>(SerializerSettings))?.Mod;
        }

        return null;
    }
}

public record ModEncapsulation(ModDetails Mod);
public record ModDetails(List<ReleaseInfo> Releases);
public record ReleaseInfo(int ReleaseId, Uri MainFile, HashSet<string> Tags, string ModVersion);