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

We will now see how to capture stills and videos.

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

## Going deeper

The document [CameraInsights](CameraInsights.md) provides detailed information on:

- The two different camera stacks in the Raspbian Operating System
- Why I decided to rely upon the native `libcamera-apps` instead of the `libcamera library` APIs
- How to switch from/to the two camera stacks
