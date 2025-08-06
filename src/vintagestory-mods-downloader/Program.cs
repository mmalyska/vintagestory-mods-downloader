using System.Text.Json;
using System.Web;
using VintagestoryModsDownloader;

Console.WriteLine("Getting list of mods...");
var filepath = Environment.GetEnvironmentVariable("config-file") ?? "mods.json";
var latestVersion = Environment.GetEnvironmentVariable("vs-version") ?? await GetLatestStable();
var downloadPath = Environment.GetEnvironmentVariable("download-path") ?? "./mods";
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
    var mods = await JsonSerializer.DeserializeAsync<HashSet<ModInput>>(File.OpenRead(filepath), serializerSettings) ?? throw new NullReferenceException();
    var directory = Directory.CreateDirectory(downloadPath);
    foreach (var file in directory.EnumerateFiles())
    {
        file.Delete();
    }
    foreach (var mod in mods)
    {
        Console.WriteLine($"{mod.Name}: {mod.Id}");
        Uri? modUri;
        if (mod.Version != null)
            modUri = await GetSelectedVersionDownloadUri(modClient, mod);
        else
            modUri = await GetLatestDownloadUri(modClient, mod);
        Console.WriteLine(modUri);
        await DownloadMod(modUri, directory);
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

async Task DownloadMod(Uri? downloadUri, DirectoryInfo storagePath)
{
    var filename = HttpUtility.ParseQueryString(downloadUri?.Query ?? "").Get("dl") ?? downloadUri?.AbsolutePath ?? "";
    if (storagePath.EnumerateFiles(filename).Any())
    {
        Console.WriteLine($"File {filename} already exists.");
        return;
    }
    using var client = new HttpClient();
    using var response = await client.GetAsync(downloadUri);
    await using var fileStream = File.Create(Path.Combine(storagePath.FullName, Path.GetFileName(filename)));
    await response.Content.CopyToAsync(fileStream);
    Console.WriteLine($"Downloaded {filename}");
}

async Task<Uri?> GetLatestDownloadUri(ModHttpClient modHttpClient, ModInput mod)
{
    var details = await modHttpClient.GetModDetails(mod.Id);
    var latestSemVer = Semver.SemVersion.Parse(latestVersion, Semver.SemVersionStyles.Strict);
    if (details?.Releases is null)
    {
        Console.WriteLine($"No releases found for mod {mod.Id}");
    }
    else
    {
        var releases = details.Releases.Where(x => x.SemVerTags.Any(t => t.CompareSortOrderTo(latestSemVer) <= 0));
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

async Task<Uri?> GetSelectedVersionDownloadUri(ModHttpClient modHttpClient, ModInput mod)
{
    var details = await modHttpClient.GetModDetails(mod.Id);
    var versionSemVer = Semver.SemVersion.Parse(mod.Version!, Semver.SemVersionStyles.Strict);
    if (details?.Releases is null)
    {
        Console.WriteLine($"No releases found for mod {mod.Id}");
    }
    else
    {
        var releases = details.Releases.Where(x => x.SemVer.Equals(versionSemVer));
        if (!releases.Any())
        {
            var release = details.Releases.First();
            Console.WriteLine($"Using latest release {release.ModVersion}");
            return release.MainFile;
        }
        else
        {
            var release = releases.First();
            Console.WriteLine($"Using pinned release {release.ModVersion}");
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