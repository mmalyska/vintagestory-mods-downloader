// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using System.Text.Json.Serialization;
using vintagestory_mods_downloader;

Console.WriteLine("Getting list of mods...");
var filepath = Environment.GetEnvironmentVariable("filepath") ?? "mods.json";
var latestVersion = Environment.GetEnvironmentVariable("vs-version") ?? await GetLatestStable();
Console.WriteLine($"Using latest VS version: {latestVersion}");

if (!File.Exists(filepath))
{
    Console.WriteLine("Mods file not found.");
    return;
}

var serializerSettings = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

using var modClient = new ModHttpClient();

try
{
    var mods = await JsonSerializer.DeserializeAsync<HashSet<ModInput>>(File.OpenRead(filepath), serializerSettings);
    foreach (var mod in mods)
    {
        Console.WriteLine($"{mod.Name}: {mod.Id}");
        var modUri = await GetLatestDownloadUri(modClient, mod);
        Console.WriteLine(modUri);
    }
}
catch (JsonException e)
{
    Console.WriteLine(e.Message);
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

return;

async Task<Uri?> GetLatestDownloadUri(ModHttpClient modHttpClient, ModInput mod)
{
    var details = await modHttpClient.GetModDetails(mod.Id);
    if (details?.Releases is null)
    {
        Console.WriteLine($"No releases found for mod {mod.Id}");
    }
    else
    {
        var releases = details.Releases.Where(x => x.Tags.Contains(latestVersion));
        if (!releases.Any())
        {
            var release = details.Releases.First();
            Console.WriteLine($"Using latest release {release.ModVersion}");
            return release.MainFile;
        }
        else
        {
            var release = releases.First();
            Console.WriteLine($"Using matched release {release.ModVersion}");
            return release.MainFile;
        }
    }

    return null;
}

async Task<string> GetLatestStable()
{
    using var client = new HttpClient();
    return await client.GetStringAsync("https://api.vintagestory.at/lateststable.txt");
}