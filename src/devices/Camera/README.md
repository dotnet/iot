# The Camera binding

This binding allows to capture still images or videos by running the tools shipped with the Raspbian Operating System by the Raspberry PI Foundation. This binding focuses on the applications capturing still pictures or videos, including both the `raspicam` and `libcamera` applications mentioned below.

Some of the classes included in this binding (like `ProcessRunner`) may be used to retrieve the output of any application writing to `stdout`, as most of the Linux terminal applications do.

This binding includes:

- The `ProcessRunner` class that takes care of the invocation of an external process and controlling its execution.
  - The `ProcessSettings` holds the settings used to configure the inter-process communication using the Linux pipes.
  - The `ProcessSettingsFactory` is a factory class for the `ProcessSettings` specifically targeting the still and video capturing apps available on the Raspbian OS.
- The `CameraInfo` class that parses the output from `--list-cameras` and retrieve the basic information about the installed cameras
- The `Command*` types represent the options to be passed to the `libcamera-apps` command line.
  - The `CommandOptionsBuilder` class allows to easily set up the most popular options without having to remember the command line or hardcoding the strings in the code.
- The `LibcameraAppsSettings` contains the list of all the available known options of the apps shipped with the Raspbian OS at the time of writing.

## Motivations

This section explains the reasons behind the choice to interface the Camera device using an external process instead of using the APIs.

### The video stack provided by the Raspberry Foundation

Sometime ago, the Raspberry PI foundation [declared the historical camera APIs as legacy](https://www.raspberrypi.com/documentation/computers/camera_software.html#libcamera-and-libcamera-apps). In some `Raspbian OS` images there is still the possibility to switch the camera stack to the legacy mode and use the support (`VideoDevice` class) that we had for years in the `dotnet/iot` library.

Since the transition to `libcamera`, the APIs are now very different. While in theory this is a great news, the decision from the `libcamera` team to only provide a `C++ API` without any `C language` wrapper makes the interoperability with any language very difficult.

> The `C++ language`, does not provide a stable binary interface (**`ABI`**) over time. This means that if you use a different compiler brand or even just a different compiler version, the binaries may be different. The consequence is breaking the compatibility and making the caller application crash.
>
> To avoid this problem, you have to recompile the C++ library from the sources every time. Anyway, recompiling all of the `libcamera` source code in the `dotnet/iot` pipeline would heavily affect the performance of the CI/CD process and it was a unanimous decision to avoid that.
>
> There is also an additional issue with recompiling from the sources. Should we do it, we should also maintain all the different version with different binaries because we do not have control over the `libcamera` version shipped (or upgraded) in the `Raspbian OS`.

This problem is not something limited to the `.NET` world, but any language. For example [this Rust library](https://github.com/lit-robotics/libcamera-rs) have the same issues and created a `C API` wrapper for their own use. C++ is a great language choice, but unfortunately it makes the interoperability with other languages extremely difficult.

#### LibCamera Compatibility Layer

The `libcamera` API provides a sort of compatibility layer which aimed to expose an API compatible with the legacy API. But [the tests I did](https://github.com/dotnet/iot/issues/1875#issuecomment-1156406558) proved that this is not a viable solution. The compatibility layer did not reach a maturity level sufficient to provide a stable and working solution.

#### Writing a C Wrapper

Since `libcamera` does not provide a `C language` wrapper around the `C++ library`, this can be written separately from anyone. While this is a possible solution, there is still the problem to maintain aligned the `C wrapper` with the `C++ compilation` which ends up in the same nightmare of interfacing directly C# with the C++ [mangled names](https://en.wikipedia.org/wiki/Name_mangling).

Also, since `libcamera` has not reached yet a stable release, the API shape could still change at any moment in time making this task more difficult.

#### Why is the Python wrapper successful

The Raspberry Foundation successfully provides a Python library. This may raise the question why we could not do the same. They could do it more easily because they ship together the Raspbian Operating System, the `libcamera` library and the wrapper, making the version alignment straightforward.

On our side, we have no visibility over the upcoming API/ABI changes and we would need to maintain several versions to support current and past versions of the library shipped by the OS or available as an upgrade (using `apt-get`). This makes the task harder for both the maintainers (`dotnet/iot`) and the users which should download the matching binary version every time.

#### H.264 hardware encoder

From our investigation, the H.264 hardware encoder available on the Raspberry PI is **not** supported by `libcamera`. This means that, even using `libcamera`,  after acquiring the video stream, it is still necessary to do additional steps to interop with the `HW` integrated circuit to encode the video and finally rebuild the video stream.

Instead, the `libcamera-apps` that are built and shipped by the `Raspberry Foundation` do support the H264 the hardware encoding available on the Raspberry PI boards.

This means that the interoperability with `libcamera` is just one of the tasks to do in order to obtain the equivalent functionalities offered by the `libcamera-apps` run as external processes.

## The simplest possible solution

Creating a library which directly call the APIs may be slightly better for the performance, but Linux provides a very efficient inter-process communication via `pipes` that drive `stdout` data exchange.

This binding takes care to execute the utilities provided by the operating system that use `libcamera` and the `H.264` hardware video encoding. The application's output is then sent to `stdout` and read from the code in this binding.

The advantage of this solution is the ability to use all the features exposed by the `libcamera-apps`. Applications like `libcamera-still`, `libcamera-vid` and `libcamera-raw` can be configured with their own command line and the output captured in C#.

## How does the capture work?

The flowchart is pretty simple:

- An application creates an instance of the `ProcessRunner` which takes care of running an external executable.
- Upon the invocation of one of the execution methods, the process will be executed with the provided command line arguments, which decide the capture options and behavior.
- The only requirement to use this strategy is that the tool must support writing its output to `stdout`. For example the `libcamera-still` or `libcamera-vid` **must use** the `-o -` option to write the stills or the video stream to `stdout`.

> To avoid messing with the command line arguments, the `CommandOptionsBuilder` class provides a convenient fluent-API.

The expected output from the process determines which method should be used:

- When running `libcamera-still` or `libcamera-vid` with `--list-cameras`, the expected output is text and contains the list of installed cameras and their characteristics. This content should be read using one of the `ExecuteReadString` methods.
- When running `libcamera-still` or `libcamera-vid` in capture mode, a binary still image or video stream is written on `stdout`. This content should be read by creating a (file or memory) Stream and passed to one of the `ExecuteAsync` methods.
- Depending on the command line, the process could run in continuous mode (it should be stopped manually) or it will self-terminate after a given time. The `ProcessRunner` class allows to eventually stop 'on-demand' the process when it is capturing continuously.

In the Unit tests, the exchange of data through `stdin` and `stdout` is validated through the `FakeVideoCapture` console application. This application takes a file in input and writes its content to `stdout` using `2Kb` chunks. The unit tests run this process and read its `stdin` to verify that all the content is correctly processed.

## Setting up the correct command line options

The `raspistill`, `libcamera-still`, `raspivid` and `libcamera-vid` share the same command line options. These options can be built using the `CommandOptionsBuilder` class instead of dealing with the strings which can be chained and used directly with the `ProcessRunner`.

The [official documentation](https://www.raspberrypi.com/documentation/computers/camera_software.html#common-command-line-options) provides a detailed explanation of all the command line options available to capture stills and videos.

The `CommandOptionsBuilder` allows to chain the most popular options using a fluent API.

In the constructor it adds by default the the option `--output -` which stands for "redirect all the output to `stdio`". This can be turned off by passing `false` to the constructor.

```csharp
var timeout = CommandOptionsBuilder.Create(Command.Timeout, "5000");
var builder = new CommandOptionsBuilder(false)
            .With(timeout);
var args = builder.GetArguments();
```

The class also exposes some methods adding groups of popular options:

```csharp
var builder = new CommandOptionsBuilder()
    .WithContinuousStreaming()
    .WithH264VideoOptions("baseline", "4", 15)
    .WithResolution(640, 480);
var args = builder.GetArguments();
```

It's worth noting any option with the same value will be added only once even if it gets added multiple times in the fluent API.

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

```csharp
ProcessSettings settings = new()
{
    Filename = "libcamera-vid",
    WorkingDirectory = null,
};

using var proc = new ProcessRunner(settings);
var text = await proc.ExecuteReadOutputAsStringAsync(args);
```

## Switching the camera stack to `libcamera` or legacy in Raspbian

Depending on the `OS` release, the capture drivers are either the legacy `raspi*` or the newest `libcamera`.

The operating system version can be checked using the following command:

```bash
cat /etc/os-release
```

The `OS` versions are listed here: [Raspbian OS versions](https://www.raspberrypi.com/software/operating-systems/)

| OS Version | Codename | Default camera stack |
| ---------- | -------- | -------------------- |
| 9          | Stretch  | legacy               |
| 10         | Buster   | legacy               |
| 11         | Bullseye | `libcamera`          |
| 12         | Bookworm | `libcamera`          |

The utilities to capture pictures or videos are fully [described in the documentation](https://www.raspberrypi.com/documentation/computers/camera_software.html). The two main utilities are the following:

| Feature        | `raspicam` utilities | `libcamera` utilities |
| -------------- | -------------------- | --------------------- |
| Still pictures | `raspistill`         | `libcamera-still`     |
| video          | `raspivid`           | `libcamera-vid`       |

The command line described in the documentation is very similar for both the stacks.

From `Bullseye` on, the `raspi-config` app allows to re-enable or disable the legacy camera stack.

```bash
sudo raspi-config
```

Then choose `Interface Options` and finally `Legacy Camera` (which refers to the `raspicam` old stack).

## Capturing stills and videos

Once the `ProcessRunner` has been created and the command line has been configured (manually or via the `CommandOptionsBuilder`), we can finally capture stills or videos.

### Preparing the application stack

The `ProcessSettingsFactory` exposes a few methods to prepare an instance of the `ProcessSettings` class with the correct application name.

```csharp
var processSettings = ProcessSettingsFactory.CreateForLibcamerastill();
```

In addition to the application name, the `ProcessSettings` has other two parameters:

- `BufferSize` is the size of the buffer used when copying data from `stdin` and the target stream
- `WorkingDirectory` is the directory used when running the process. The `null` value will default to `Environment.CurrentDirectory`. In any case the still and video apps are available through the environment path therefore this parameter is typically not used.
- `CaptureStderrInsteadOfStdout` is a Boolean flag indicating whether the output to be captured is `stderr` instead of `stdout`. For example, when capturing the text for the available cameras (`--list-cameras`) the output is sent to `stderr` and not to `stdout`.

### Capturing stills

The first step to capture stills is to prepare the command line. For example:

```csharp
var builder = new CommandOptionsBuilder()
    .WithTimeout(1)
    .WithVflip()
    .WithHflip()
    .WithPictureOptions(90, "jpg")
    .WithResolution(640, 480);
var args = builder.GetArguments();
```

Then we create an instance of the `ProcessRunner`:

```csharp
using var proc = new ProcessRunner(processSettings);
```

Finally we run the process redirecting the stream appropriately. This code will write the data coming from the running process to the file stream opened for write.

```csharp
var filename = CreateFilename("jpg");
using var file = File.OpenWrite(filename);
await proc.ExecuteAsync(args, file);
```

### Capturing a video

Similarly, we can capture the video preparing the command line options, creating the `ProcessRunner` and preparing the destination file:

```csharp
var builder = new CommandOptionsBuilder()
    .WithContinuousStreaming(0)
    .WithVflip()
    .WithHflip()
    .WithResolution(640, 480);
var args = builder.GetArguments();
using var proc = new ProcessRunner(processSettings);

var filename = CreateFilename("h264");
using var file = File.OpenWrite(filename);
```

We can then capture the output in a similar way we did with the stills. Otherwise we can use a "continuous mode" which consists in offloading the capture on a separate thread.

```csharp
var task = await proc.ContinuousRunAsync(args, file);
await Task.Delay(5000);
proc.Dispose();
// The following try/catch is needed to trash the OperationCanceledException triggered by the Dispose
try
{
    await task;
}
catch (Exception)
{
}

return filename;
```

The task named `task` refers to the thread creation operation. After this we await for 5 seconds and then call `Dispose` which will send a `SIGKILL` signal to the external process.

The `try/catch` block is necessary because the `Dispose` method also cancel the asynchronous stream copy using a cancellation token. This block will silently swallow the `OperationCanceledException`.

> The provided sample code also shows how to capture a time-lapse video (a progression of pictures saved on the file system at a regular interval).

### Listing the available cameras

This code is only supported with the `libcamera` stack and can be used with the `libcamera-still` or `libcamera-vid` applications, but remember that those two apps send the text output on `stderr` and not `stdout`.

```csharp
var processSettings = ProcessSettingsFactory.CreateForLibcamerastillAndStderr();
using var proc = new ProcessRunner(processSettings);
var text = await proc.ExecuteReadOutputAsStringAsync(string.Empty);
IEnumerable<CameraInfo> cameras = await CameraInfo.From(text);
```

The list of `CameraInfo` is obtained by running the app with the `--list-cameras`, parsing the output and getting the most important values:

- `Index` is the index of the camera in the list. This is used in the `CommandOptionsBuilder.WithCamera` method to specify a camera index greater than `0`
- `Name`: the name of the camera
- `MaxResolution`: a string with the horizontal and vertical resolution
- `DevicePath`: the Linux device path of the camera