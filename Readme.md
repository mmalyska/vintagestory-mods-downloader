# Vintage Story Mods Downloader

A console application for automatically downloading mods for Vintage Story game from the official repository, based on configuration in the `mods.json` file.

## Description

Vintage Story Mods Downloader is a tool that allows for automatic downloading of mods for Vintage Story game. Instead of manually downloading each mod, you can define a list of mods in a JSON file and download them all with a single command. The application automatically:

- Retrieves current information about mods from the official Vintage Story API
- Matches appropriate mod versions to the selected game version
- Allows specifying specific mod versions
- Organizes downloaded mods in a specified directory

## Runtime requirements
- .NET 9.0
- Windows/Linux/macOS system

## Configuration

The application uses the `mods.json` file to determine which mods should be downloaded. The file should contain a list of mods with their identifiers from the official Vintage Story repository.

### Example mods.json file

```json
[
  {
    "name": "Better Storage",
    "id": "betterstorage"
  },
  {
    "name": "Primitive Survival",
    "id": "primitivesurvival",
    "version": "2.3.1"
  },
  {
    "name": "Wildcraft",
    "id": "wildcraft"
  }
]
```

### Field descriptions:
- `name` - friendly name of the mod (used only for display)
- `id` - mod identifier from the Vintage Story repository
- `version` - optional, specific mod version to download (if not specified, the latest version compatible with the selected game version is downloaded)

## Usage

### Basic usage
```bash
dotnet run
```

### Environment variables
The application supports the following environment variables:

- `config-file` - path to the configuration file (default `mods.json`)
- `vs-version` - Vintage Story version (by default, the latest stable version is downloaded)
- `download-path` - path to the directory where mods will be saved (default `./mods`)

Example:
```bash
config-file=server-mods.json vs-version=1.18.8 download-path=./server/mods dotnet run
```

### With Docker
```bash
docker run \
  -v $(pwd)/mods.json:/app/mods.json \
  -v $(pwd)/mods:/app/mods \
  -e vs-version=1.18.8 \
  vs-mods-downloader
```

## Usage examples

### 1. Downloading mods for the latest version

File `mods.json`:
```json
[
  {
    "name": "Creative Tools",
    "id": "creativetools"
  },
  {
    "name": "World Edit",
    "id": "worldedit"
  }
]
```

Command:
```bash
dotnet run
```

### 2. Server modpack for a specific game version

File `server-mods.json`:
```json
[
  {
    "name": "Server Essentials",
    "id": "serveressentials"
  },
  {
    "name": "Teleportation",
    "id": "teleportation"
  },
  {
    "name": "Wildcraft",
    "id": "wildcraft",
    "version": "3.1.5"
  }
]
```

Command:
```bash
vs-version=1.18.5 config-file=server-mods.json download-path=./server/mods dotnet run
```

## How does it work?

1. The application retrieves information about the latest stable version of Vintage Story (or uses the version specified by the user)
2. For each mod on the list:
   - Retrieves mod details from the Vintage Story API
   - If a specific version is specified, downloads that version
   - Otherwise, selects the latest version compatible with the selected game version
   - Downloads the mod file and saves it in the specified directory

## Troubleshooting

### Error: "Mods file not found"
Make sure that the `mods.json` file exists in the working directory or specify the correct path using the `config-file` environment variable.

### Download error
Check if:
- The mod ID is correct
- You have an internet connection
- The Vintage Story API is available

### "No releases found for mod"
Check if the mod with the given identifier exists in the Vintage Story repository.

## Development

### Requirements
- .NET 9.0
- C# 13.0
- HttpClient for API communication
- System.Text.Json for JSON parsing
- Semver library for semantic versioning

### Compilation from source
```bash
git clone <repository-url>
cd vintagestory-mods-downloader
dotnet build
```

### Running in Docker
```bash
docker build -t vs-mods-downloader .
docker run -v $(pwd)/mods.json:/app/mods.json -v $(pwd)/mods:/app/mods vs-mods-downloader
```

## License

[License information]