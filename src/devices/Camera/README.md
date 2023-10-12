# Camera

This binding allows to capture still images or videos by running the tools shipped with the Raspbian Operating System by the Raspberry PI Foundation.

The same criteria can be used with any other tool that is able to write the binary still or video stream to `stdout`.

This binding includes:

- The `ProcessRunner` class that takes care of the invocation of an external process and controlling its execution.
- The `CameraInfo` class that parses the output from `--list-cameras` and retrieve the basic information about the installed cameras
- 

## Motivations

Sometime ago, the Raspberry PI foundation [declared the historical camera APIs as legacy](https://www.raspberrypi.com/documentation/computers/camera_software.html#libcamera-and-libcamera-apps). In some `Raspbian` images there is still the possibility to switch the camera stack to the legacy mode and use the support (`VideoDevice` class) that we had for years in the `dotnet/iot` library.

Since the transition to `libcamera` the APIs are now very different. While in theory this is a great news, the decision from the `libcamera` team to only provide a `C++  API` without any `C API` makes the interoperability with any language very difficult.

> The `C++ language`, does not provide a stable binary interface (`ABI`) over time. This means that if you use a different compiler brand or even just a different version, the binaries are different.
>
> This is not a problem if you always recompile the `C++` library but recompiling all of the `libcamera` source code in the `dotnet/iot` pipeline would heavily affect the performance of the CI/CD process.

This problem is not something limited to the `.NET` world, but any language. For example [this Rust library](https://github.com/lit-robotics/libcamera-rs) have the same issues and created a `C API` wrapper for their own use.

#### Writing a C Wrapper

Writing a `C language` wrapper is a possible solution but it is time expensive and does not avoid the versioning issues as the `libcamera` library at the moment has not reached a stable release and their API could change at any moment in time.

#### LibCamera Compatibility Layer

Lastly, the `libcamera` API provides a sort of compatibility layer which aimed to expose an API compatible with the legacy API. But [I did several tests](https://github.com/dotnet/iot/issues/1875#issuecomment-1156406558) and verified it was not evolved enough to provide a stable and working solution.

#### Python Wrapper

The Raspberry Foundation is successfully providing a Python library, how does it work. Since they ship the Raspbian Operating System which contains the `libcamera` library, it is much easier for them to align the Python wrapper as soon as the `libcamera` binaries change because of a different compiler version or because the API has changed.

We instead have no visibility over the upcoming changes and would need to maintain several versions to support current and past versions of the library.

#### H.264 encoder

From our investigation, the H.264 hardware encoder available on the Raspberry PI is **not** supported by `libcamera`. This means that, even using `libcamera`,  after acquiring the video stream, it is still necessary to do additional steps to interop with the `HW IC` to encode the video and finally rebuild the video stream. 

## The easier path

Creating a library which directly call the APIs is definitely better for the performance, but using the `stdout pipe` available on Linux is not that bad.

This binding takes care to execute the utilities provided by the operating system that use `libcamera` and also provide the `H.264` hardware encoding. The output is sent to `stdout` and read from the code in this binding.

This is simplest possible solution which also provide all the features offered by the `libcamera-apps`. Applications like `libcamera-still`, `libcamera-vid` and `libcamera-raw` can be configured with their own command line and the output captured in C#.

## How does the capture work?

The flowchart is pretty simple:

- An application creates an instance of the `ProcessRunner` which takes care of running an external executable.
- Upon the invocation of one of the execution methods, the process will be run with the provided command line arguments, which decides its behavior.
- The only requirement for this process is to write its its output on `stdout`. For example the `libcamera-still` or `libcamera-vid` **must use** the `-o -` command line to write the stills or the video stream to `stdout`

The expected output from the process determines which methods should be used:

- When running `libcamera-still` or `libcamera-vid` with `--list-cameras`, the expected output is text and contains the list of installed cameras and their characteristics. This content should be read using one of the `ExecuteReadString` methods.
- When running `libcamera-still` or `libcamera-vid` in capture mode, a binary still image or video stream is written on `stdout`. This content should be read by creating a (file or memory) Stream and passed to one of the `ExecuteAsync` methods.
- Depending on the command line, the process could run in continuous mode (it should be stopped manually) or it will self-terminate after a given time. The `ProcessRunner` class allows to eventually forcibly stop the process when it is capturing continuously and should be stopped.

In the Unit tests, the exchange of data through `stdin` and `stdout` is validated through the `FakeVideoCapture` console application. This application takes a file in input and writes its content to `stdout` using `2Kb `chunks. The unit tests run this process and read its `stdin` to verify that all the content is correctly processed.



