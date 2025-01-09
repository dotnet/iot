# Example commands for deploying to Raspberry Pi

## Build / Publish

Run from repository root (below: ```C:\code\iot```)

```pwsh
# Change '--no-self-contained' to '--self-contained' if runtime is not installed on Pi
dotnet publish C:\code\iot\src\devices\Display\samples\LedSegmentDisplay5641AS.Sample\LedSegmentDisplay5641AS.Sample.csproj `
    -c Debug -f net6.0 -o debug -r linux-arm64 --no-self-contained
```

## Deploy

Create folder on Pi: `led-display`

```pwsh
sftp user@192.168.1.xxx
put -R C:\code\iot\debug\* led-display
# or just changes to sample project:
# put C:\code\iot\debug\Led* led-display
```

## Run

```pwsh
ssh user@192.168.1.xxx
cd led-display
chmod +x LedSegmentDisplay5641AS.Sample
./LedSegmentDisplay5641AS.Sample
# CTRL+C to end
```

## Debug

[Debug .NET Core on Linux](https://learn.microsoft.com/en-us/visualstudio/debugger/remote-debugging-dotnet-core-linux-with-ssh?view=vs-2022)

Ensure PDB files are created and deployed.
