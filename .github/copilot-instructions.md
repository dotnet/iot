# .NET IoT — GitHub Copilot Coding Agent Instructions

**ALWAYS reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

This repository contains the .NET IoT libraries: **System.Device.Gpio** (low-level GPIO APIs) and **Iot.Device.Bindings** (130+ device drivers/bindings for sensors, displays, motors, etc.). Code runs across different operating systems and hardware platforms.

## Working Effectively with Fresh Clone

### Required Dependencies and Setup
- **CRITICAL**: This repository requires **Azure DevOps feeds access** to build. If you get SDK resolution errors, this is due to network restrictions.
- **Install .NET 8.0.300 SDK**: Use `./dotnet.sh --version` to verify the exact version from global.json is available
- **Build command**: `./build.sh --restore --build` (Linux/macOS) or `Build.cmd` (Windows)
- **NEVER CANCEL**: Full repository build takes **30-45 minutes**. Set timeout to **60+ minutes**.

### Bootstrap and Build Process
Run these commands in order:
```bash
# 1. Ensure correct .NET SDK version (may download SDK automatically)
./dotnet.sh --version

# 2. Full repository build - NEVER CANCEL, takes 30-45 minutes
./build.sh --restore --build --configuration Release

# 3. Build with packages (for testing distribution)
./build.sh --restore --build --pack --configuration Release

# 4. Run tests - NEVER CANCEL, takes 15-30 minutes  
./build.sh --test --configuration Release
```

**If Azure DevOps feeds are inaccessible**: Build will fail with "Could not resolve SDK Microsoft.DotNet.Arcade.Sdk" errors. Document this as a known limitation requiring network access to Azure DevOps.

### Alternative Build Approaches
When main build fails due to network restrictions:
```bash
# Build individual samples (these use public NuGet packages)
cd samples/led-blink
../../dotnet.sh build

# Test basic .NET IoT functionality
./dotnet.sh new console -o test-project
cd test-project  
./dotnet.sh add package System.Device.Gpio
./dotnet.sh add package Iot.Device.Bindings
./dotnet.sh build && ./dotnet.sh run
```

## Repository Structure and Navigation

### Key Directories
- **`src/System.Device.Gpio/`** - Core GPIO library (pins, controllers, drivers)
- **`src/devices/`** - 130+ device-specific bindings (sensors, displays, motors)
- **`samples/`** - Standalone example projects demonstrating usage
- **`Documentation/`** - Comprehensive docs, contribution guidelines, tutorials
- **`tools/`** - Build tools, Arduino C# compiler, code generators

### Important Files
- **`global.json`** - Specifies required .NET SDK version (8.0.300)
- **`build.sh`/`Build.cmd`** - Main build scripts using Microsoft Arcade SDK
- **`azure-pipelines.yml`** - Official CI/CD pipeline configuration
- **`Directory.Build.props`** - Shared MSBuild properties and package references

### Finding Device Bindings
```bash
# List all 130+ available device bindings
ls src/devices/

# Find specific device (e.g., temperature sensors)
find src/devices -name "*temp*" -o -name "*dht*" -o -name "*bmp*"

# Each device has: library code, samples/, tests/, README.md
ls src/devices/Dhtxx/  # Example: DHT temperature sensor
```

## Testing and Validation

### Running Tests
```bash
# Run all tests - NEVER CANCEL, takes 15-30 minutes
./build.sh --test --configuration Release

# Run specific test project
./dotnet.sh test src/devices/Dhtxx/tests/

# Test count: 31 test projects across the repository
find . -name "*Test*.csproj" | wc -l
```

### Manual Validation Scenarios
**ALWAYS run these validation steps after making changes:**

1. **Build validation**: Ensure your changes don't break the overall build
2. **Sample execution**: Run relevant sample projects to verify functionality
3. **Hardware-independent testing**: Focus on parsing, calculation, and protocol logic
4. **Cross-platform considerations**: Verify code works on Linux, Windows, and embedded systems

### Validation for Device Bindings
```bash
# After modifying a device binding (e.g., Dhtxx):
cd src/devices/Dhtxx
../../../dotnet.sh build

# Test the sample
cd samples
../../../../dotnet.sh build
# Note: Hardware may not be available, verify compilation and logic

# Check for hardware abstraction compliance
grep -r "Environment.OSVersion\|DllImport\|P/Invoke" . # Should be minimal/none
```

## Common Commands and Expected Timing

| Command | Expected Time | Timeout Setting |
|---------|---------------|-----------------|
| `./build.sh --restore --build` | 30-45 minutes | 60+ minutes |
| `./build.sh --test` | 15-30 minutes | 45+ minutes |
| Individual device build | 1-3 minutes | 10 minutes |
| Sample project build | 30-60 seconds | 5 minutes |
| Full CI pipeline | 45-60 minutes | 90+ minutes |

### Linting and Code Quality
```bash
# Run markdown linting (used in CI)
npm install -g markdownlint-cli
markdownlint -c .markdownlint.json .

# Code follows .editorconfig and built-in analyzers
# Warnings/errors will appear during build - fix them
```

## Debugging Build Issues

### Common Issues and Solutions
1. **SDK resolution errors**: Azure DevOps feeds inaccessible - document as network limitation
2. **Package not found**: Check if using correct NuGet sources in NuGet.config
3. **Memory issues**: Use `/maxCpuCount:1` to disable parallel builds if needed
4. **Cross-platform issues**: Device bindings should use System.Device.* abstractions only

### Log Locations
- Build logs: `artifacts/log/{Configuration}/`
- Test results: Published in CI, available in pipeline artifacts

## Working with Device Bindings

### Device Binding Patterns (follow existing conventions)
1. **Constructor**: Accept `I2cDevice`, `SpiDevice`, `GpioController` etc. from caller
2. **API Design**: Use `TryRead*` methods for sensor readings, clear property names  
3. **Resource Management**: Implement `IDisposable`, dispose hardware resources properly
4. **Error Handling**: Validate inputs, handle hardware failures gracefully
5. **Documentation**: XML docs with units (°C, Pa, %RH), README with wiring info
6. **Samples**: Always include runnable sample demonstrating typical usage

### Quick Device Binding Development
```bash
# Create new device binding using template
cd src/devices
# TODO: Use template when available, or copy from existing device

# Essential files for new device:
# - DeviceName.csproj
# - DeviceName.cs (main class)
# - samples/DeviceName.samples.csproj
# - samples/Program.cs
# - README.md
```

## Package and Distribution

### NuGet Packages Produced
- **System.Device.Gpio** - Core GPIO abstractions and drivers
- **Iot.Device.Bindings** - All device bindings in single package  
- **Iot.Device.Bindings.SkiaSharpAdapter** - Graphics/display adapter

### Package Sources
- **Release builds**: NuGet.org (public)
- **Nightly builds**: GitHub Packages (requires authentication)
- **Development**: Local artifacts/packages/{Configuration}/

## Important Notes for Copilot Coding Agent

### DO
- **Build early and often** - catch issues before they compound
- **Focus on hardware abstraction** - use System.Device.* APIs exclusively
- **Test compilation** even without hardware - logic should be testable
- **Follow existing patterns** - browse src/devices/ for examples
- **Document thoroughly** - XML docs, README files, units in comments
- **Handle disposal correctly** - hardware resources must be properly released

### DON'T  
- **Cancel long-running builds** - they may take 45+ minutes normally
- **Hardcode platform-specific paths** - maintain cross-platform compatibility
- **Skip sample projects** - they're essential for demonstrating usage
- **Introduce breaking changes** - this is experimental but stable-in-spirit API
- **Use P/Invoke or OS-specific APIs** - defeats hardware abstraction purpose

### Critical Success Factors
1. **Network Access**: Azure DevOps feeds required for full build capability
2. **Patience**: Builds are lengthy due to large codebase (130+ bindings)
3. **Hardware Awareness**: Code should work without hardware for testing
4. **Cross-platform Mindset**: Support Linux, Windows, embedded systems equally

---

## Scope & context (what this repo is)
- This repo contains the low-level **System.Device.Gpio** APIs and a large collection of device drivers under **Iot.Device.Bindings**.
- Code is used across different OSes and boards. Prefer **hardware-agnostic** code, with platform-specific behavior isolated behind drivers/abstractions.
- Treat public APIs as **experimental but stable-in-spirit**: prefer additive changes, avoid breaking patterns unless maintainers direct otherwise.

---

## Build, test, and layout (how to work here)
- **Solution layout:** library code lives under `src/` (e.g., `src/System.Device.Gpio`, `src/devices/<DeviceName>`); runnable examples live under `samples/` and often under each device's folder.
- **Build:** Use repository build scripts (`build.sh`/`Build.cmd`) for full builds. Individual projects can use `./dotnet.sh build` when network allows.
- **Tests:** prefer **unit tests** for logic that can be isolated from hardware. For hardware-only functionality, ensure samples (and documentation) make manual verification straightforward.
- **Docs:** device-specific docs/README typically live next to the device code; repo-wide docs live under `Documentation/`.

> Copilot: when proposing commands or CI steps, use repository build scripts first, then individual `./dotnet.sh build` commands. Do not bump target frameworks or change solution structure unless explicitly asked.

---

## Coding style & conventions (how to write code)
- Follow the repo's **.editorconfig** and standard .NET naming rules (PascalCase for public members/types, camelCase for locals/fields; interfaces start with `I`).
- Prefer **clarity over cleverness**: braces for all control flow, avoid long chained expressions; add small helper methods if they improve readability.
- Enable and fix compiler warnings/analyzers; keep `using` directives tidy.
- Nullability: write **null-safe** code and validate inputs (`ArgumentNullException`, `ArgumentOutOfRangeException`, etc.).
- Avoid unnecessary allocations in hot paths; prefer spans/stackalloc/pooled buffers when reading/writing registers or doing tight loops.

---

## Resource management (very important for hardware)
- Any type that owns hardware resources (GPIO pins, I2C/SPI devices, file handles, PWM, etc.) must implement **`IDisposable`** and release resources promptly.
- Dispose/close transports (`I2cDevice`, `SpiDevice`, `GpioController`) you create. If a transport is passed in by the caller, **do not dispose it** unless documented as owned.
- Design for **failure**: ensure exceptions (or `Try*` patterns) leave the device in a consistent state; clean up on error.

---

## Device binding design (patterns Copilot should follow)
When adding or updating a **device binding** under `src/devices/<DeviceName>`:

1. **Constructor & dependencies**
   - Accept dependencies that the caller can configure, e.g. `I2cDevice`, `SpiDevice`, `GpioController`, pin numbers, bus addresses, etc.
   - Provide sensible defaults (e.g., common I²C addresses), but never hardcode board-specific details.
   - Avoid singletons and implicit global state.

2. **API shape**
   - Prefer **`TryRead*`** methods that return `bool` and set `out` parameters for sensor readings (e.g., `TryReadTemperature(out Temperature value)`).
   - For settings and commands, use clear methods (e.g., `Reset()`, `EnableHeater(bool)`) or strongly-typed properties. Use enums/flags for registers, modes, scales.
   - Keep synchronous APIs **predictable**; only introduce `async` if there's genuine I/O concurrency or long waits.

3. **Timing & reliability**
   - Respect datasheet timing: enforce minimal delays between transactions; document any polling limits.
   - Validate checksums/status bits where appropriate and surface failures clearly.

4. **Error handling**
   - Guard against misuse (invalid pins, uninitialized device) with `ArgumentException`/`InvalidOperationException` and descriptive messages.
   - For transient hardware issues, either throw or return `false` in `Try*` patterns—**never** return bogus data silently.

5. **Cross-platform**
   - Use **System.Device** abstractions (controllers, drivers) instead of P/Invoke or OS-specific syscalls. If a feature is platform-only, guard it and fail gracefully elsewhere.

6. **Samples & docs**
   - Add a **sample** demonstrating typical usage (`samples/` and/or `src/devices/<DeviceName>/samples`). Keep samples small, readable, and disposable-friendly.
   - Add or update a **README** for the device with wiring notes, required pull-ups, expected ranges/units, and example code.

---

## Don'ts (things Copilot should avoid suggesting)
- Don't propose **board-specific hacks** or hardcoded paths to `/sys`/proc unless the existing pattern does so via an abstraction.
- Don't capture `Console` I/O in library code (leave printing/logging to samples/apps).
- Don't add hidden thread‑static caches or background threads without clear ownership and disposal.
- Don't change package versions, TFMs, or CI configuration unless requested.
- Don't introduce breaking API changes to public types without a strong justification and maintainer direction.

---

## Review checklist for new/updated bindings (use this before finishing a PR)
- [ ] Public API follows repo naming/style; members are XML-documented with units (°C, Pa, %RH, lux, etc.).
- [ ] Constructor accepts caller‑provided transports and documents ownership.
- [ ] Implements `IDisposable`; all hardware resources released.
- [ ] Uses enums/consts for registers/addresses; no unexplained magic numbers.
- [ ] Validates timing and checksums; `Try*` methods return accurate success/failure.
- [ ] Adds/updates **samples** and **device README**.
- [ ] Adds tests for logic that can be isolated from hardware (e.g., framing/parsing functions).

---

## Good prompt patterns (how to ask Copilot)
Use explicit, repo‑aware prompts. Examples:

- *"Create a new I2C temperature sensor binding under `src/devices/AcmeTemp` with a `TryReadTemperature(out Temperature t)` method and a sample app. Constructor should accept an `I2cDevice` and an optional address (default 0x48). Implement `IDisposable` and document units."*
- *"Refactor existing SPI display code to avoid per-frame allocations. Suggest a pooled buffer approach using `Span<byte>` and show before/after microbenchmarks."*
- *"Add XML docs and a README section for BMP280 explaining oversampling settings and expected units. Update the sample to dispose controllers correctly."*

---

## Where to look in this repo (for Copilot context)
- **Patterns:** browse `src/devices` for existing sensor/display bindings; mirror their constructors, enums, and `Try*` APIs.
- **Abstractions:** use `System.Device.Gpio`, `System.Device.I2c`, `System.Device.Spi`, `System.Device.Pwm` where possible.
- **Docs:** check `Documentation/` for contribution guidelines; keep wording and tone consistent.

---

## Security, licensing, and compliance
- Code is under **MIT**; include appropriate headers where needed and avoid copying vendor code without license compatibility.
- If a binding requires vendor blobs/firmware, keep them **out** of the repo; link to vendor downloads and document setup.

---

## One‑liner summary (what Copilot should optimize for)
> **Write clear, disposable-correct, hardware-agnostic device code that matches existing patterns, surfaces failures honestly, and ships with samples and docs.**