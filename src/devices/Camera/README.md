# The Camera binding

This binding allows to capture still images or videos by running the tools shipped with the Raspbian Operating System by the Raspberry PI Foundation.

The same criteria can be used with any other tool that is able to write the binary still or video stream to `stdout`.

This binding includes:

- The `ProcessRunner` class that takes care of the invocation of an external process and controlling its execution.
- The `CameraInfo` class that parses the output from `--list-cameras` and retrieve the basic information about the installed cameras
- The `Command*` types represent the options to be passed to the `libcamera-apps` command line. The `CommandOptionsBuilder` class allows to easily set up the most popular options without having to remember the command line or hardcoding the strings in the code.
- The `LibcameraAppsSettings` contains the list of all the available known options of the apps shipped with the Raspbian OS at the time of writing.
- The `ProcessSettings` class contains the settings used to configure the inter-process communication using the Linux pipes.

## Motivations

Sometime ago, the Raspberry PI foundation [declared the historical camera APIs as legacy](https://www.raspberrypi.com/documentation/computers/camera_software.html#libcamera-and-libcamera-apps). In some `Raspbian OS` images there is still the possibility to switch the camera stack to the legacy mode and use the support (`VideoDevice` class) that we had for years in the `dotnet/iot` library.

Since the transition to `libcamera`, the APIs are now very different. While in theory this is a great news, the decision from the `libcamera` team to only provide a `C++ API` without any `C language` wrapper makes the interoperability with any language very difficult.

> The `C++ language`, does not provide a stable binary interface (**`ABI`**) over time. This means that if you use a different compiler brand or even just a different compiler version, the binaries may be different. The consequence is breaking the compatibility and making the caller application crash.
>
> To avoid this problem, you have to recompile the C++ library from the sources every time. Anyway, recompiling all of the `libcamera` source code in the `dotnet/iot` pipeline would heavily affect the performance of the CI/CD process and it was a unanimous decision to avoid that.
>
> There is also an additional issue with recompiling from the sources. Should we do it, we should also maintain all the different version with different binaries because we do not have control over the `libcamera` version shipped (or upgraded) in the `Raspbian OS`.

This problem is not something limited to the `.NET` world, but any language. For example [this Rust library](https://github.com/lit-robotics/libcamera-rs) have the same issues and created a `C API` wrapper for their own use.

#### Writing a C Wrapper

Writing and maintaining our own `C language` wrapper is a possible solution but it is time expensive and does not avoid the versioning issues mentioned above. Furthermore, the `libcamera` library at the moment has not reached a stable release and their API could change at any moment in time.

#### LibCamera Compatibility Layer

The `libcamera` API provides a sort of compatibility layer which aimed to expose an API compatible with the legacy API. But [I did several tests](https://github.com/dotnet/iot/issues/1875#issuecomment-1156406558) and verified it was not evolved enough to provide a stable and working solution.

#### Python Wrapper

Since the Raspberry Foundation successfully provides a Python library, you may wonder why we could not do the same. Since they ship the Raspbian Operating System which contains the `libcamera` library, it is much easier for them to align the Python wrapper version to the matching `libcamera` binaries whereas this should change the API or its ABI.

On our side, we have no visibility over the upcoming changes and we would need to maintain several versions to support current and past versions of the library shipped by the OS or available as an upgrade (using `apt-get`).

#### H.264 encoder

From our investigation, the H.264 hardware encoder available on the Raspberry PI is **not** supported by `libcamera`. This means that, even using `libcamera`,  after acquiring the video stream, it is still necessary to do additional steps to interop with the `HW` integrated circuit to encode the video and finally rebuild the video stream. 

Instead, the `libcamera-apps` that are shipped by the `Raspberry Foundation` do support the H264 hardware encoding.

## The simplest possible solution

Creating a library which directly call the APIs may be slightly better for the performance, but using the `stdout pipe` available on Linux is not that bad.

This binding takes care to execute the utilities provided by the operating system that use `libcamera` and also provide the `H.264` hardware encoding. The output is sent to `stdout` and read from the code in this binding.

The advantage of this solution is the ability to use all the features exposed by the `libcamera-apps`. Applications like `libcamera-still`, `libcamera-vid` and `libcamera-raw` can be configured with their own command line and the output captured in C#.

## How does the capture work?

The flowchart is pretty simple:

- An application creates an instance of the `ProcessRunner` which takes care of running an external executable.
- Upon the invocation of one of the execution methods, the process will be executed with the provided command line arguments, which decide the capture options and behavior.
- The only requirement to use this strategy is that the tool must support writing its output to `stdout`. For example the `libcamera-still` or `libcamera-vid` **must use** the `-o -` option to write the stills or the video stream to `stdout`

The expected output from the process determines which method should be used:

- When running `libcamera-still` or `libcamera-vid` with `--list-cameras`, the expected output is text and contains the list of installed cameras and their characteristics. This content should be read using one of the `ExecuteReadString` methods.
- When running `libcamera-still` or `libcamera-vid` in capture mode, a binary still image or video stream is written on `stdout`. This content should be read by creating a (file or memory) Stream and passed to one of the `ExecuteAsync` methods.
- Depending on the command line, the process could run in continuous mode (it should be stopped manually) or it will self-terminate after a given time. The `ProcessRunner` class allows to eventually forcibly stop the process when it is capturing continuously and should be stopped.

In the Unit tests, the exchange of data through `stdin` and `stdout` is validated through the `FakeVideoCapture` console application. This application takes a file in input and writes its content to `stdout` using `2Kb ` chunks. The unit tests run this process and read its `stdin` to verify that all the content is correctly processed.

## Setting up the correct command line options

The class `CommandOptionsBuilder` simplify the construction of the command line which is common to almost all the `libcamera-apps` applications.

This is not a required step as the user may manually build the string with the help of the [official documentation](https://www.raspberrypi.com/documentation/computers/camera_software.html#common-command-line-options) and pass it to the execution functions of the `ProcessRunner` class.

Instead, the `CommandOptionsBuilder` allows to build the most popular options with the following code:

```csharp
var output = CommandOptionsBuilder.Create(Command.Output, "-");
var builder = new CommandOptionsBuilder()
    .With(output);
```

The class also exposes some methods adding groups of popular options:

```csharp
var builder = new CommandOptionsBuilder()
    .WithContinuousStreaming()
    .WithH264VideoOptions("baseline", "4", 15)
    .WithResolution(640, 480);
var args = builder.GetArguments();
```

It's worth noting that options with the same value will be added only once even if it gets added multiple times.

## Verify the command line

If something goes wrong during the execution of the process, there are two ways to verify the correctness of the command line.

**The first** is to retrieve the full command line from the `ProcessRunner`:

```csharp
var builder = new CommandOptionsBuilder()
    .WithContinuousStreaming();
var args = builder.GetArguments();

ProcessSettings settings = new()
{
    Filename = "libcamera-vid",
    WorkingDirectory = null,
};

using var proc = new ProcessRunner(settings);
var fullCommandLine = proc.GetFullCommandLine(args);
```

The `fullCommandLine` obtained from the `ProcessRunner` can now be executed manually in a terminal to verify the output from the `libcamera-vid` application and fix the values for the command line options.

It's worth noting that the result may change depending on the `WorkingDirectory`. When `null` is specified, the process is executed using `Environment.CurrentDirectory`.

> Usually it is not necessary to run the `libcamera-apps` as `root`.

**The second** is retrieving the text output from the execution. If the process printed an error message, this should be available through the following code:

```
ProcessSettings settings = new()
{
    Filename = "libcamera-vid",
    WorkingDirectory = null,
};

using var proc = new ProcessRunner(settings);
var text = await proc.ExecuteReadOutputAsStringAsync(args);
```