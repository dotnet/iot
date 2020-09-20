How to Contribute
=================

# Building IoT Repo

## Recommended Software
1. (If building on Windows) - **[Visual Studio 2019](https://visualstudio.microsoft.com/vs/preview/)** (Community, Professional, Enterprise) with the latest updates. This is not required for building the repo, but it will help a lot if the intent is to change the code or add new bindings.
1. **.NET Core SDK** To ensure the install worked, make sure you can call `dotnet` tool from a command prompt. We now require .NET 5 preview versions of the SDKs because of our central infrastructure, you can find the installers per platform here:
 - For Windows: [Link to .NET 5 preview installer for Windows](https://dotnetcli.azureedge.net/dotnet/Sdk/5.0.100-preview.6.20310.4/dotnet-sdk-5.0.100-preview.6.20310.4-win-x64.exe) **This is important as you won't be able to open projects on Visual Studio if you don't install this machine-wide**
 - For Linux: [Link to .NET 5 preview SDK for Linux](https://dotnetcli.azureedge.net/dotnet/Sdk/5.0.100-preview.6.20310.4/dotnet-sdk-5.0.100-preview.6.20310.4-linux-x64.tar.gz)
 - For OSX: [Link to .NET 5 preview SDK for OSX](https://dotnetcli.azureedge.net/dotnet/Sdk/5.0.100-preview.6.20310.4/dotnet-sdk-5.0.100-preview.6.20310.4-osx-x64.tar.gz)

## Building from the Command Line

From a (non-admin) Command Prompt window (or a Terminal shell when building on Linux/OSX):

- `build.cmd` (or `./build.sh` on Linux/OSX) -  Will cause basic tool initialization and build the main library (System.Device.Gpio), the device bindings (projects under src/devices), tools and samples.

For more information on how to turn of parts of the build, please type `build.cmd -h` (or `./build.sh -h` on Linux/OSX) to see an up-to-date list

## Building from Visual Studio (Windows only)

**Make sure you have the .NET 5 preview SDK installed machine-wide in order to be able to open projects in VisualStudio or else you will get project loading problems. You can find the link above.**

In order to be able to open and build most projects in the repo with Visual Studio, you would have to already have run `build.cmd` from the root at least once. This is because this command will restore basic tooling and SDKs that most of the repo relies on. Once you have done that once, you should be able to open individual projects with Visual Studio, and should be able to build them in the IDE.

When opening the main library project (System.Device.Gpio) in Visual Studio, you can select the right configuration depending if you want to build the Linux or the Windows Configuration:

![](images/configurations.png)

It is worth noting that files which are specific to the Linux configuration of a project, will have a filename like `*.Linux.cs` while the ones that are specific to Windows would have it like `*.Windows.cs` instead.
