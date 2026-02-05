---
name: debug-build-issues
description: Guide for diagnosing and resolving build, compilation, and SDK issues in the .NET IoT repository
license: MIT
---

## Purpose

This skill helps Copilot agents diagnose and fix build issues in the .NET IoT repository, which has specific requirements around Azure DevOps feeds, .NET SDK versions, and lengthy build times.

## When to Use This Skill

- Build fails with SDK resolution errors
- Package restore failures
- Compilation errors after making changes
- Long-running build timeouts
- CI/CD pipeline failures
- Memory issues during build

## Instructions

### 1. Understanding Build Requirements

**CRITICAL Prerequisites:**
- Azure DevOps feeds access required for full builds
- .NET 8.0.300 SDK (specified in `global.json`)
- Build takes 30-45 minutes normally - **NEVER CANCEL**
- Test runs take 15-30 minutes

### 2. Common Build Issues and Solutions

#### SDK Resolution Errors
**Symptom:** `Could not resolve SDK Microsoft.DotNet.Arcade.Sdk`

**Diagnosis:**
```bash
# Check SDK version
./dotnet.sh --version

# Verify global.json
cat global.json
```

**Solutions:**
1. Azure DevOps feeds are inaccessible (network restriction)
   - Document as known limitation
   - Try alternative build approaches (see below)
2. SDK version mismatch
   - Ensure .NET 8.0.300 SDK is installed
   - `./dotnet.sh --version` will auto-download if needed

#### Package Not Found Errors
**Symptom:** NuGet package restore failures

**Diagnosis:**
```bash
# Check NuGet sources
cat NuGet.config

# Check network connectivity to Azure DevOps
# (feeds are at Azure DevOps, not public NuGet)
```

**Solutions:**
1. Verify NuGet.config has correct sources
2. Check if Azure DevOps feeds are accessible
3. Try clearing NuGet cache: `dotnet nuget locals all --clear`

#### Memory Issues During Build
**Symptom:** Out of memory errors, process killed

**Solutions:**
```bash
# Disable parallel builds
./build.sh --restore --build /maxCpuCount:1

# Or build smaller subsets
cd src/devices/Bmp280
../../../dotnet.sh build
```

#### Timeout Issues
**Symptom:** Build appears hung or times out

**Important:** Builds legitimately take 30-45 minutes
- Set timeout to **60+ minutes** for full builds
- Set timeout to **45+ minutes** for test runs
- Use `initial_wait` parameter appropriately in bash commands

### 3. Alternative Build Approaches

When Azure DevOps feeds are inaccessible:

#### Build Individual Samples
```bash
# Samples use public NuGet packages
cd samples/led-blink
../../dotnet.sh build
```

#### Test Basic IoT Functionality
```bash
./dotnet.sh new console -o test-project
cd test-project
./dotnet.sh add package System.Device.Gpio
./dotnet.sh add package Iot.Device.Bindings
./dotnet.sh build && ./dotnet.sh run
```

#### Build Individual Device Bindings
```bash
cd src/devices/<DeviceName>
../../../dotnet.sh build
```

### 4. Build Commands by Scenario

#### Full Repository Build
```bash
# Use this for complete validation
./build.sh --restore --build --configuration Release
# Expected time: 30-45 minutes
# Timeout setting: 60+ minutes
```

#### Build with Packages
```bash
# For testing distribution
./build.sh --restore --build --pack --configuration Release
```

#### Run Tests
```bash
# All tests
./build.sh --test --configuration Release
# Expected time: 15-30 minutes

# Specific test project
./dotnet.sh test src/devices/Dhtxx/tests/
```

#### Individual Device Build
```bash
cd src/devices/Dhtxx
../../../dotnet.sh build
# Expected time: 1-3 minutes
```

### 5. Debugging Compilation Errors

#### After Code Changes
```bash
# Build the specific project you changed
cd src/devices/<DeviceName>
../../../dotnet.sh build

# Check for warnings/errors in output
# Fix compiler warnings - they're required by analyzers
```

#### Check for Common Issues
```bash
# Verify .editorconfig compliance
# Build will show analyzer warnings

# Check for using directive issues
# Ensure proper namespace imports

# Validate nullability
# Code should be null-safe with proper annotations
```

### 6. CI/CD Pipeline Issues

When GitHub Actions workflows fail:

```bash
# Check workflow status
gh run list --limit 5

# View specific run logs
gh run view <run-id> --log

# Common CI issues:
# - Markdown linting failures
# - Build timeouts (increase timeout)
# - Test failures (check if pre-existing)
```

#### Markdown Linting
```bash
npm install -g markdownlint-cli
markdownlint -c .markdownlint.json .
```

### 7. Log Locations

When builds fail, check logs:
- Build logs: `artifacts/log/{Configuration}/`
- Test results: Published in CI artifacts
- Individual project logs: In project's bin/obj folders

### 8. Cross-Platform Build Issues

#### Linux vs Windows Issues
- Check for platform-specific files: `*.Linux.cs` vs `*.Windows.cs`
- Verify configurations in Visual Studio
- Use `System.Device.*` abstractions, not OS-specific APIs

### 9. Diagnostic Commands

```bash
# Check repository status
git --no-pager status
git --no-pager diff

# Verify .NET installation
./dotnet.sh --version
./dotnet.sh --info

# Check for stale build artifacts
ls -la artifacts/

# Clean build
rm -rf artifacts/
./build.sh --restore --build --configuration Release

# Check for platform abstraction violations
grep -r "Environment.OSVersion\|DllImport\|P/Invoke" src/devices/<DeviceName>/
```

### 10. When to Escalate

Document and inform user when:
- Azure DevOps feeds are confirmed inaccessible (network policy)
- Build requires infrastructure changes
- Pre-existing test failures unrelated to changes
- Issues require maintainer intervention

## Troubleshooting Workflow

1. **Identify the error** - Read error messages carefully
2. **Check prerequisites** - Verify SDK version, network access
3. **Try minimal build** - Build just the affected component
4. **Check for pre-existing issues** - Build before your changes
5. **Clear and rebuild** - Remove artifacts and try clean build
6. **Use alternative approaches** - Try sample builds if main build blocked
7. **Document limitations** - Note if Azure DevOps access is issue

## Expected Timing Reference

| Command | Expected Time | Timeout Setting |
|---------|---------------|-----------------|
| `./build.sh --restore --build` | 30-45 minutes | 60+ minutes |
| `./build.sh --test` | 15-30 minutes | 45+ minutes |
| Individual device build | 1-3 minutes | 10 minutes |
| Sample project build | 30-60 seconds | 5 minutes |
| Full CI pipeline | 45-60 minutes | 90+ minutes |

## References

- [Copilot Instructions - Build Section](../../copilot-instructions.md#bootstrap-and-build-process)
- [Contributing Guidelines](../../../Documentation/CONTRIBUTING.md)
- [Azure Pipelines Config](../../../azure-pipelines.yml)
