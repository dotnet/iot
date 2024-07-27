# How to Contribute

## Recommended Software

1. (If building on Windows) - **[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)** (Community, Professional, Enterprise) with the latest updates.  
(Alternatively, on any platform) - **[Visual Studio Code](https://code.visualstudio.com/download)** with the **[C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)** extension.  
These are not required for building the repo, but they will help a lot if the intent is to change the code or add new bindings.
1. **.NET SDK** To ensure the install worked, make sure you can call the `dotnet` tool from a command prompt. We recommend a current release version of the SDK because of our central infrastructure. You can find the supported releases [here](https://dotnet.microsoft.com/en-us/download/dotnet). For each supported release there are installers for Windows, Linux, and MacOS.

## Building from the Command Line

From a (non-admin) Command Prompt on Windows or a (non-root) shell when building on Linux/OSX):

- `build.cmd` on Windows, or `./build.sh` on Linux/OSX -  Will cause basic tool initialization and build the main library (System.Device.Gpio), the device bindings (projects under src/devices), tools and samples.

For more information on how to turn of parts of the build, please type `build.cmd -h` (or `./build.sh -h` on Linux/OSX) to see an up-to-date list.

## Building from Visual Studio Code

Open a Terminal within Visual Studio Code and follow the instructions for building from the command line.

## Building from Visual Studio (Windows only)

**Make sure you have the .NET SDK installed machine-wide in order to be able to open projects in Visual Studio or else you will get project loading problems. You can find the link above.**

In order to be able to open and build most projects in the repo with Visual Studio, you would have to already have run `build.cmd` from the root at least once. This is because this command will restore basic tooling and SDKs that most of the repo relies on. Once you have done that once, you should be able to open individual projects with Visual Studio, and should be able to build them in the IDE.

When opening the main library project (System.Device.Gpio) in Visual Studio, you can select the right configuration depending if you want to build the Linux or the Windows Configuration:

![configurations](images/configurations.png)

It is worth noting that files which are specific to the Linux configuration of a project, will have a filename like `*.Linux.cs` while the ones that are specific to Windows would have it like `*.Windows.cs` instead.

To work with individual bindings, each folder under `src/devices/` has a solution that includes all relevant projects for that binding, together with an example project using the binding and the unit tests (if any). These solutions are the preferred way of working with individual bindings.
