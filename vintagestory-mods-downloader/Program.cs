// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using System.Text.Json.Serialization;
using vintagestory_mods_downloader;

Console.WriteLine("Getting list of mods...");
var filepath = Environment.GetEnvironmentVariable("filepath") ?? "mods.json";
if (!File.Exists(filepath))
{
    Console.WriteLine("Mods file not found.");
    return;
}

var serializerSettings = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

var modClient = new ModHttpClient();

try
{
    var mods = await JsonSerializer.DeserializeAsync<HashSet<ModInput>>(File.OpenRead(filepath), serializerSettings);
    foreach (var mod in mods)
    {
        Console.WriteLine($"{mod.Name}: {mod.Id}");
        var details = await modClient.GetModDetails(mod.Id);
        if (details?.Releases is null)
        {
            Console.WriteLine($"No releases found for mod {mod.Id}");
        }
        else
        {
            Console.WriteLine($"{details.Releases.Count} releases");
        }
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