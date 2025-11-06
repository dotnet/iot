# Creating an IoT Project with .NET

This guide walks you through creating a .NET IoT application from scratch, covering both command-line and Visual Studio approaches.

## Prerequisites

### Hardware

- Raspberry Pi (3, 4, or 5) or compatible single-board computer
- MicroSD card (16GB+ recommended) with Raspberry Pi OS installed
- Power supply (5V/3A minimum, 5V/5A for Pi 5)
- Basic electronics kit (breadboard, LEDs, resistors, jumper wires)

### Software

- **.NET 8.0 SDK** or later installed on Raspberry Pi
- SSH access (optional but recommended for development)

## Installing .NET on Raspberry Pi

### Method 1: Using Installation Script (Recommended)

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
```

Add to PATH:

```bash
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc
```

### Method 2: Using Package Manager

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt update
sudo apt install dotnet-sdk-8.0
```

### Verify Installation

```bash
dotnet --version
```

Should output: `8.0.xxx` or later.

## Creating a Project with .NET CLI

### Step 1: Create Console Application

```bash
# Create project directory
mkdir MyIotApp
cd MyIotApp

# Create console application
dotnet new console
```

This creates:

- `MyIotApp.csproj` - Project file
- `Program.cs` - Main program file

### Step 2: Add IoT Packages

```bash
# Add System.Device.Gpio (core GPIO library)
dotnet add package System.Device.Gpio

# Optional: Add device bindings
dotnet add package Iot.Device.Bindings
```

### Step 3: Write Code

Edit `Program.cs`:

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

Console.WriteLine("Blinking LED. Press Ctrl+C to exit.");

// Create GPIO controller
using GpioController controller = new();

// Configure GPIO 18 as output
const int ledPin = 18;
controller.OpenPin(ledPin, PinMode.Output);

// Blink LED
while (true)
{
    controller.Write(ledPin, PinValue.High);
    Console.WriteLine("LED ON");
    Thread.Sleep(1000);
    
    controller.Write(ledPin, PinValue.Low);
    Console.WriteLine("LED OFF");
    Thread.Sleep(1000);
}
```

### Step 4: Build and Run

```bash
# Build project
dotnet build

# Run application
dotnet run
```

Press `Ctrl+C` to stop.

## Creating a Project with Visual Studio

### Prerequisites

- Visual Studio 2022 or later
- .NET 8.0 SDK or later
- Remote debugging tools (optional)

### Step 1: Create New Project

1. Open Visual Studio
2. **File** → **New** → **Project**
3. Search for "Console App"
4. Select **Console App (.NET)** (not .NET Framework)
5. Click **Next**

### Step 2: Configure Project

1. **Project name:** `MyIotApp`
2. **Location:** Choose directory
3. **Framework:** `.NET 8.0` or later
4. Click **Create**

### Step 3: Add NuGet Packages

1. Right-click project in Solution Explorer
2. Select **Manage NuGet Packages**
3. Click **Browse** tab
4. Search for `System.Device.Gpio`
5. Click **Install**
6. Optionally search and install `Iot.Device.Bindings`

### Step 4: Write Code

Replace contents of `Program.cs` with IoT code (same as CLI example above).

### Step 5: Build

1. **Build** → **Build Solution** (or press `Ctrl+Shift+B`)

### Step 6: Deploy and Run

#### Option A: Copy Binaries to Raspberry Pi

1. Build project in Visual Studio
2. Find binaries in `bin/Debug/net8.0/` or `bin/Release/net8.0/`
3. Copy entire folder to Raspberry Pi via SCP/SFTP
4. SSH into Raspberry Pi
5. Navigate to copied folder
6. Run: `dotnet MyIotApp.dll`

#### Option B: Remote Debugging (Advanced)

See [Remote Debugging Guide](https://learn.microsoft.com/en-us/dotnet/iot/debugging?tabs=self-contained)

## Project Structure

### Minimal Project

```
MyIotApp/
├── MyIotApp.csproj
├── Program.cs
└── bin/
    └── Debug/
        └── net8.0/
```

### Recommended Project Structure

```
MyIotApp/
├── MyIotApp.csproj
├── Program.cs
├── README.md
├── .gitignore
├── Devices/              # Device-specific code
│   ├── LedController.cs
│   └── SensorReader.cs
├── Configuration/        # App configuration
│   └── appsettings.json
└── Tests/               # Unit tests
    └── MyIotApp.Tests.csproj
```

## Project Configuration

### Adding Configuration File

Create `appsettings.json`:

```json
{
  "Hardware": {
    "LedPin": 18,
    "ButtonPin": 17
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

Update `.csproj` to copy config file:

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

Install configuration package:

```bash
dotnet add package Microsoft.Extensions.Configuration.Json
```

Use in code:

```csharp
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

int ledPin = config.GetValue<int>("Hardware:LedPin");
```

### Adding Logging

```bash
dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Extensions.Logging.Console
```

Use in code:

```csharp
using Microsoft.Extensions.Logging;

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("MyIotApp");

logger.LogInformation("Application started");
logger.LogWarning("Button pressed");
```

## Dependency Injection (Advanced)

For larger projects, use dependency injection:

```bash
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.DependencyInjection
```

Create `Worker.cs`:

```csharp
using Microsoft.Extensions.Hosting;
using System.Device.Gpio;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly GpioController _gpio;

    public Worker(ILogger<Worker> logger, GpioController gpio)
    {
        _logger = logger;
        _gpio = gpio;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _gpio.OpenPin(18, PinMode.Output);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _gpio.Write(18, PinValue.High);
            _logger.LogInformation("LED ON");
            await Task.Delay(1000, stoppingToken);
            
            _gpio.Write(18, PinValue.Low);
            _logger.LogInformation("LED OFF");
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

Update `Program.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Device.Gpio;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<GpioController>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
```

Run as systemd service (see [Systemd Guide](deployment/systemd-services.md)).

## Publishing for Deployment

### Self-Contained Deployment

Includes .NET runtime (larger but no dependencies):

```bash
# For ARM32 (Pi 3, Zero)
dotnet publish -c Release -r linux-arm

# For ARM64 (Pi 4, Pi 5)
dotnet publish -c Release -r linux-arm64

# Output in: bin/Release/net8.0/linux-arm64/publish/
```

### Framework-Dependent Deployment

Smaller but requires .NET runtime on target:

```bash
dotnet publish -c Release
```

### Copy to Raspberry Pi

```bash
# Using SCP
scp -r bin/Release/net8.0/linux-arm64/publish/* pi@raspberrypi.local:~/MyIotApp/

# Or using rsync
rsync -av bin/Release/net8.0/linux-arm64/publish/ pi@raspberrypi.local:~/MyIotApp/
```

### Run on Raspberry Pi

```bash
ssh pi@raspberrypi.local
cd ~/MyIotApp
./MyIotApp  # Self-contained
# or
dotnet MyIotApp.dll  # Framework-dependent
```

## Adding Unit Tests

Create test project:

```bash
dotnet new xunit -n MyIotApp.Tests
cd MyIotApp.Tests
dotnet add reference ../MyIotApp.csproj
dotnet add package Moq
```

Example test:

```csharp
using Xunit;
using System.Device.Gpio;
using Moq;

public class LedControllerTests
{
    [Fact]
    public void TurnOn_ShouldWriteHigh()
    {
        // Arrange
        var mockGpio = new Mock<GpioController>();
        var controller = new LedController(mockGpio.Object);

        // Act
        controller.TurnOn();

        // Assert
        mockGpio.Verify(g => g.Write(It.IsAny<int>(), PinValue.High), Times.Once);
    }
}
```

Run tests:

```bash
dotnet test
```

## Common Project Patterns

### Pattern 1: Simple Script

Single `Program.cs` file, no structure. Good for:

- Quick prototypes
- Learning
- Simple one-off scripts

### Pattern 2: Device Abstraction

Separate device logic from main program:

```csharp
// Devices/Led.cs
public class Led : IDisposable
{
    private readonly GpioController _gpio;
    private readonly int _pin;

    public Led(GpioController gpio, int pin)
    {
        _gpio = gpio;
        _pin = pin;
        _gpio.OpenPin(_pin, PinMode.Output);
    }

    public void On() => _gpio.Write(_pin, PinValue.High);
    public void Off() => _gpio.Write(_pin, PinValue.Low);
    
    public void Dispose() => _gpio.ClosePin(_pin);
}

// Program.cs
using var gpio = new GpioController();
using var led = new Led(gpio, 18);
led.On();
```

### Pattern 3: Service-Based Architecture

Use .NET Generic Host with services:

- Background workers
- Dependency injection
- Configuration
- Logging

Good for production applications.

## Development Workflow

### Local Development (Cross-Platform)

Develop on Windows/Mac/Linux, deploy to Raspberry Pi:

1. Write code in Visual Studio/VS Code
2. Test logic with mock GPIO (unit tests)
3. Build for target architecture
4. Copy to Raspberry Pi
5. Test on real hardware
6. Repeat

### Remote Development

Use VS Code with SSH:

1. Install "Remote - SSH" extension in VS Code
2. Connect to Raspberry Pi
3. Open folder
4. Edit and run code directly on Pi

### Docker Development

Develop in containers (see [Containers Guide](deployment/containers.md)).

## Best Practices

1. ✅ **Use `using` statements** for automatic disposal
2. ✅ **Handle exceptions** gracefully (hardware can fail)
3. ✅ **Log important events** for debugging
4. ✅ **Validate hardware configuration** at startup
5. ✅ **Make pin numbers configurable** (don't hardcode)
6. ✅ **Add README** with wiring instructions
7. ✅ **Version control** your code (Git)
8. ✅ **Test incrementally** (one device at a time)

## Example: Complete Project Template

```bash
# Create solution
dotnet new sln -n MyIotSolution

# Create main project
dotnet new console -n MyIotApp
dotnet sln add MyIotApp/MyIotApp.csproj

# Create test project
dotnet new xunit -n MyIotApp.Tests
dotnet sln add MyIotApp.Tests/MyIotApp.Tests.csproj
cd MyIotApp.Tests
dotnet add reference ../MyIotApp/MyIotApp.csproj
cd ..

# Add packages
cd MyIotApp
dotnet add package System.Device.Gpio
dotnet add package Iot.Device.Bindings
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Logging.Console
cd ..

# Build solution
dotnet build
```

## Troubleshooting

### "Project doesn't build"

```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

### "GPIO doesn't work on PC"

GPIO requires Linux + real hardware. Use mocking for development on PC.

### "Application runs slow"

Build in Release mode:

```bash
dotnet build -c Release
dotnet run -c Release
```

## Next Steps

- [GPIO Basics](fundamentals/gpio-basics.md) - Learn GPIO programming
- [Understanding Protocols](fundamentals/understanding-protocols.md) - I2C, SPI, UART
- [Device Bindings](../src/devices/README.md) - Pre-built sensor libraries
- [Deployment Guide](deployment/containers.md) - Deploy applications
- [Troubleshooting](troubleshooting.md) - Fix common issues

## Additional Resources

- [.NET IoT Documentation](https://docs.microsoft.com/dotnet/iot/)
- [Device Bindings Samples](../src/devices)
- [.NET CLI Reference](https://docs.microsoft.com/dotnet/core/tools/)
- [Visual Studio IoT Development](https://learn.microsoft.com/visualstudio/iot/)
