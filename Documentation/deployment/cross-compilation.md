# How to Deploy an IoT App

This provides information on how to publish and deploy a .NET IoT application to a development board like a Raspberry Pi.

## Using .NET CLI (Recommended)

### Publish for Raspberry Pi

```bash
# Framework-dependent (requires .NET runtime on target)
dotnet publish -c Release -r linux-arm64 --self-contained false

# Self-contained (includes .NET runtime - larger but no runtime needed)
dotnet publish -c Release -r linux-arm64 --self-contained true
```

**Common Runtime Identifiers (RIDs):**

| Target                          | RID           |
| ------------------------------- | ------------- |
| Raspberry Pi 3/4/5 (64-bit OS)  | `linux-arm64` |
| Raspberry Pi 3/Zero (32-bit OS) | `linux-arm`   |

### Deploy to Raspberry Pi

**Using SCP (Secure Copy):**

```bash
# Copy published files to Raspberry Pi
scp -r bin/Release/net8.0/linux-arm64/publish/* pi@raspberrypi.local:~/myiotapp/

# SSH into Pi and run
ssh pi@raspberrypi.local
cd ~/myiotapp
chmod +x MyIotApp
./MyIotApp
```

**Using rsync (faster for updates):**

```bash
rsync -avz bin/Release/net8.0/linux-arm64/publish/ pi@raspberrypi.local:~/myiotapp/
```

## Using Visual Studio

### Publish Profile

1. Right-click the project in Solution Explorer → **Publish...**
2. Select **Folder** as the target
3. Click **Configure...**
4. Set the following:
   - **Target Framework:** net8.0
   - **Deployment Mode:** Framework-dependent or Self-contained
   - **Target Runtime:** linux-arm64 (for 64-bit Pi) or linux-arm (for 32-bit Pi)
5. Click **Publish**

### Example Publish Profile (.pubxml)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <PublishProtocol>FileSystem</PublishProtocol>
    <Configuration>Release</Configuration>
    <Platform>Any CPU</Platform>
    <TargetFramework>net8.0</TargetFramework>
    <PublishDir>bin\publish</PublishDir>
    <RuntimeIdentifier>linux-arm64</RuntimeIdentifier>
    <SelfContained>false</SelfContained>
  </PropertyGroup>
</Project>
```

## Deployment Options

### Framework-Dependent Deployment

- **Smaller package size** - only your app, not the runtime
- **Requires .NET runtime on target** - install with: `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin`
- **Recommended** for development boards with internet access

### Self-Contained Deployment

- **Larger package** - includes .NET runtime (~50-80 MB)
- **No runtime installation needed** on target
- **Recommended** for embedded or offline deployments

### Single-File Publishing

Create a single executable:

```bash
dotnet publish -c Release -r linux-arm64 --self-contained true /p:PublishSingleFile=true
```

### Trimming (Reduce Size)

Trim unused code for smaller deployments:

```bash
dotnet publish -c Release -r linux-arm64 --self-contained true /p:PublishTrimmed=true
```

**Note:** Trimming may remove code accessed via reflection. Test thoroughly.

## File Transfer Tools

- **SCP/SFTP** - Built into most terminals (`scp`, `sftp`)
- **rsync** - Efficient incremental file sync
- **FileZilla** - GUI-based SFTP client
- **Visual Studio Code Remote SSH** - Edit and deploy directly

## References

- [.NET Application Deployment](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [dotnet publish command](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)
- [.NET RID Catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)
- [Visual Studio Publish Profiles](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/visual-studio-publish-profiles)

## See Also

- [Running in Docker Containers](containers.md) - Containerize your IoT apps
- [Auto-start on Boot](systemd-services.md) - Run apps automatically with systemd
