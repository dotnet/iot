# Running .NET IoT Applications in Docker Containers

This guide explains how to containerize and run .NET IoT applications in Docker on Raspberry Pi and other single-board computers.

## Why Use Containers?

**Benefits:**

- **Isolation:** Apps run in separate environments
- **Portability:** Build once, run anywhere
- **Dependency management:** All dependencies packaged together
- **Easy deployment:** Push image, pull and run
- **Version control:** Tag and rollback easily

**Considerations:**

- Requires Docker installation
- Additional configuration for hardware access
- Slightly more complex setup

## Prerequisites

### Install Docker on Raspberry Pi

**For 32-bit Raspberry Pi OS (armhf):**

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
```

**For 64-bit Raspberry Pi OS (arm64):**

Same as above, or use package manager:

```bash
sudo apt update
sudo apt install docker.io
sudo usermod -aG docker $USER
```

**Verify installation:**

```bash
docker --version
docker run hello-world
```

Log out and back in for group changes to take effect.

## Quick Start: Containerize Your App

### Method 1: Using .NET SDK Container Building Tools (Recommended)

**Step 1: Add Package (only needed for .NET < 8.0.200)**

```bash
dotnet add package Microsoft.NET.Build.Containers
```

**Step 2: Publish as Container**

```bash
# For ARM64 (Raspberry Pi 4, 5)
dotnet publish --os linux --arch arm64 -c Release /t:PublishContainer

# For ARM32 (Raspberry Pi 3, Zero)
dotnet publish --os linux --arch arm -c Release /t:PublishContainer
```

This automatically:

- Creates Docker image based on project name (lowercase)
- Uses appropriate .NET runtime base image
- Tags as `latest`
- Loads into local Docker registry

**Step 3: Run Container**

```bash
# Grant access to GPIO
docker run --rm --device /dev/gpiomem myiotapp:latest
```

### Method 2: Using a Dockerfile

Create `Dockerfile` in project root:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyIotApp.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Use non-root user (security best practice)
USER $APP_UID

ENTRYPOINT ["dotnet", "MyIotApp.dll"]
```

**Build and run:**

```bash
# Build for ARM64
docker build -t myiotapp .

# Run with GPIO access
docker run --rm --device /dev/gpiomem myiotapp
```

## GPIO Access in Containers

### The Problem

Containers are isolated from the host. GPIO devices need to be explicitly mounted.

### Solution 1: Mount /dev/gpiomem (Recommended)

```bash
docker run --rm --device /dev/gpiomem myiotapp
```

**Pros:**

- Secure (limited access)
- Fast (direct memory access)
- Works with libgpiod

**Cons:**

- Requires `/dev/gpiomem` to exist (most modern systems)

### Solution 2: Mount /dev/gpiochip* (For libgpiod)

```bash
# Raspberry Pi 1-4
docker run --rm --device /dev/gpiochip0 myiotapp

# Raspberry Pi 5
docker run --rm --device /dev/gpiochip4 myiotapp

# Mount all GPIO chips
docker run --rm --device /dev/gpiochip0 --device /dev/gpiochip1 myiotapp
```

### Solution 3: Mount sysfs (Legacy, Not Recommended)

```bash
docker run --rm -v /sys:/sys --privileged myiotapp
```

**Pros:**

- Works with older sysfs driver

**Cons:**

- Requires privileged mode (security risk)
- Slower than /dev/gpiomem
- Mounts entire /sys filesystem

**Use only if other methods fail.**

## Rootless Containers (Security Best Practice)

Starting with .NET 8, official Docker images use non-root user `app` (UID 1654).

### The Permission Problem

```bash
# Inside container, check device permissions
docker run -it --rm --device /dev/gpiomem --entrypoint /bin/bash myiotapp
app@container:/app$ ls -l /dev/gpiomem
crw-rw---- 1 root gpio 245, 0 Jan 28 18:45 /dev/gpiomem
```

`/dev/gpiomem` is owned by group `gpio` (GID varies by system, commonly 997 or 996).

The container's `app` user (GID 1654) cannot access it.

### Solution: Set Container User/Group

**Find GPIO group ID on host:**

```bash
getent group gpio
# Output: gpio:x:997:pi
```

**Run container with GPIO group:**

```bash
# Use app user (1654) with gpio group (997)
docker run --rm -u "1654:997" --device /dev/gpiomem myiotapp
```

**Or add APP_UID to gpio group in Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY . .

# Add app user to GPIO group (group must exist on host)
USER root
RUN groupadd -g 997 gpio && usermod -aG gpio app
USER $APP_UID

ENTRYPOINT ["dotnet", "MyIotApp.dll"]
```

## I2C, SPI, UART in Containers

### I2C

```bash
# Mount I2C device
docker run --rm --device /dev/i2c-1 myiotapp
```

**Check user has access:**

```bash
sudo usermod -aG i2c $USER
# Find I2C group ID
getent group i2c
# Use in container: -u "1654:<i2c_gid>"
```

### SPI

```bash
# Mount SPI devices
docker run --rm \
  --device /dev/spidev0.0 \
  --device /dev/spidev0.1 \
  myiotapp
```

**Set SPI group:**

```bash
getent group spi
docker run --rm -u "1654:<spi_gid>" --device /dev/spidev0.0 myiotapp
```

### UART/Serial

```bash
# Mount serial port
docker run --rm --device /dev/ttyS0 myiotapp

# Or use symbolic link
docker run --rm --device /dev/serial0 myiotapp
```

**Set dialout group:**

```bash
getent group dialout
docker run --rm -u "1654:<dialout_gid>" --device /dev/ttyS0 myiotapp
```

### Multiple Devices

```bash
docker run --rm \
  --device /dev/gpiomem \
  --device /dev/i2c-1 \
  --device /dev/spidev0.0 \
  --device /dev/ttyS0 \
  myiotapp
```

## Docker Compose

For complex setups, use `docker-compose.yml`:

```yaml
version: '3.8'

services:
  iotapp:
    image: myiotapp:latest
    container_name: iotapp
    restart: unless-stopped
    user: "1654:997"  # app:gpio
    devices:
      - /dev/gpiomem
      - /dev/i2c-1
      - /dev/spidev0.0
    environment:
      - TZ=America/New_York
    volumes:
      - ./config:/app/config:ro
```

**Run with Compose:**

```bash
docker-compose up -d
docker-compose logs -f
docker-compose down
```

## Multi-Stage Builds

Optimize image size with multi-stage Dockerfile:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyIotApp.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish \
    --no-restore \
    --self-contained false

# Runtime stage (smaller)
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "MyIotApp.dll"]
```

**Result:** Build stage discarded, only runtime + app kept (much smaller image).

## Cross-Platform Building

Build ARM images on x86/x64 machine:

### Using Docker Buildx

```bash
# Set up buildx
docker buildx create --name mybuilder --use
docker buildx inspect --bootstrap

# Build for multiple architectures
docker buildx build \
  --platform linux/arm64,linux/arm/v7 \
  -t myiotapp:latest \
  --push \
  .
```

### Using .NET SDK

```bash
# Build ARM64 image on x86 machine
dotnet publish --os linux --arch arm64 /t:PublishContainer
```

## Transferring Images to Raspberry Pi

### Method 1: Docker Registry (Recommended)

**Push to Docker Hub:**

```bash
docker tag myiotapp:latest yourusername/myiotapp:latest
docker push yourusername/myiotapp:latest

# On Raspberry Pi
docker pull yourusername/myiotapp:latest
docker run --rm --device /dev/gpiomem yourusername/myiotapp:latest
```

### Method 2: Save and Load

```bash
# On dev machine
docker save myiotapp:latest | gzip > myiotapp.tar.gz

# Transfer to Raspberry Pi (SCP)
scp myiotapp.tar.gz pi@raspberrypi.local:~

# On Raspberry Pi
gunzip -c myiotapp.tar.gz | docker load
docker run --rm --device /dev/gpiomem myiotapp:latest
```

## Example: Complete IoT App with Docker

**Project structure:**

```
MyIotApp/
├── MyIotApp.csproj
├── Program.cs
├── Dockerfile
├── docker-compose.yml
└── .dockerignore
```

**Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyIotApp.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "MyIotApp.dll"]
```

**docker-compose.yml:**

```yaml
version: '3.8'

services:
  iotapp:
    build: .
    container_name: iotapp
    restart: unless-stopped
    user: "1654:997"
    devices:
      - /dev/gpiomem
      - /dev/i2c-1
    environment:
      - DOTNET_ENVIRONMENT=Production
```

**.dockerignore:**

```
bin/
obj/
.git/
.vs/
*.md
```

**Build and run:**

```bash
docker-compose up -d
docker-compose logs -f
```

## Debugging Containers

### View Logs

```bash
docker logs mycontainer
docker logs -f mycontainer  # Follow logs
```

### Execute Commands Inside Container

```bash
# Open shell
docker exec -it mycontainer /bin/bash

# Run command
docker exec mycontainer ls -la /dev
```

### Check Container Resources

```bash
docker stats mycontainer
docker inspect mycontainer
```

## Troubleshooting

### "Permission denied" on GPIO

**Check device permissions:**

```bash
docker exec -it mycontainer ls -l /dev/gpiomem
```

**Solution:** Set correct user/group with `-u` flag.

### "Device not found"

**Verify device exists on host:**

```bash
ls -l /dev/gpiomem /dev/i2c-* /dev/spidev*
```

**Solution:** Mount device with `--device` flag.

### Container Exits Immediately

**Check logs:**

```bash
docker logs mycontainer
```

**Common causes:**

- Application crashes (check code)
- Missing dependencies (check Dockerfile)
- Incorrect entry point

### "Cannot pull image"

**For ARM architectures:**

Ensure you're building/pulling ARM-compatible images:

```bash
docker pull --platform linux/arm64 mcr.microsoft.com/dotnet/runtime:8.0
```

## Best Practices

1. **Use multi-stage builds** - Smaller images, faster deployment
2. **Run as non-root user** - Improved security
3. **Use specific tags** - Don't rely on `latest` in production
4. **Minimize layers** - Combine RUN commands
5. **Use .dockerignore** - Faster builds, smaller context
6. **Health checks** - Monitor container health
7. **Resource limits** - Prevent resource exhaustion
8. **Logging** - Send logs to stdout/stderr

## Advanced: Health Checks

Add health check to Dockerfile:

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD dotnet /app/HealthCheck.dll || exit 1
```

Or in docker-compose.yml:

```yaml
services:
  iotapp:
    healthcheck:
      test: ["CMD", "dotnet", "/app/HealthCheck.dll"]
      interval: 30s
      timeout: 3s
      retries: 3
      start_period: 5s
```

## See Also

- [IoT Project Setup](../iot-project-setup.md) - Create .NET IoT projects
- [Systemd Services](systemd-services.md) - Auto-start containers with systemd
- [Cross-Compilation](cross-compilation.md) - Build for different architectures
- [Raspberry Pi Docker GPIO Guide](../raspi-Docker-GPIO.md) - Detailed Docker GPIO setup

## Additional Resources

- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Containerize .NET App](https://learn.microsoft.com/en-us/dotnet/core/docker/publish-as-container)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Secure Containers with Rootless](https://devblogs.microsoft.com/dotnet/securing-containers-with-rootless/)
