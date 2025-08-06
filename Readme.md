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

### With Kubernetes

You can use this tool as an init container in a Kubernetes deployment to download mods before starting your Vintage Story server:

```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: vintagestory-server
  labels:
    app: vintagestory
spec:
   replicas: 1
   updateStrategy:
      type: RollingUpdate
  selector:
    matchLabels:
      app: vintagestory
  template:
    metadata:
      labels:
        app: vintagestory
    spec:
      volumes:
        - name: mods-config
          configMap:
            name: vs-mods-config
        - name: mods-storage
          emptyDir: {}
        - name: config
          persistentVolumeClaim:
             claimName: vs-config
      initContainers:
        - name: download-mods
          image: ghcr.io/mmalyska/vintagestory-mods-downloader:latest
          env:
            - name: vs-version
              value: "1.20.12"
            - name: download-path
              value: /mods
            - name: config-file
              value: /app/mods.json
          volumeMounts:
            - name: mods-config
              mountPath: /app/mods.json
              readOnly: true
              subPath: mods.json
            - name: mods-storage
              mountPath: /mods
      containers:
        - name: vintagestory-server
          image: ghcr.io/mmalyska/vintagestory:latest
          env:
             - name: DATA_PATH
               value: /config
          args:
             - --addModPath
             - /mods
          ports:
            - containerPort: 42420
          volumeMounts:
            - name: mods-storage
              mountPath: /mods
            - name: config
              mountPath: /config
            
---
apiVersion: v1
kind: ConfigMap
metadata:
  name: vs-mods-config
data:
  mods.json: |
    [
      {
        "name": "configlib",
        "id": "1783"
      },
      {
        "name": "vsimgui",
        "id": "1745"
      }
    ]
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: vs-config
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 500Mi
---
apiVersion: v1
kind: Service
metadata:
  name: vintagestory
  labels:
    app: vintagestory
spec:
  ports:
  - port: 42420
    name: game
  selector:
    app: vintagestory
```

This setup:
1. Creates a ConfigMap with your mod configuration
2. Uses an init container to download the mods before the main container starts
3. Stores the mods in an emptyDir volume shared between the init container and the main Vintage Story server container

Example of usage in my cluster with helm chart https://github.com/mmalyska/home-ops/tree/main/cluster/games/vintagestory

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

This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0) - see the [LICENSE](LICENSE) file for details.

AGPL-3.0 is a copyleft license that requires anyone who distributes your code or a derivative work to make the source available under the same terms, including when the software is served over a network. This means that if you run a modified version of this software on a server and people interact with it there, you have to make your modified source code available to them.