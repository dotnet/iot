# Camera binding insights

This section explains the reasons behind the choice to interface the Camera device using an external process instead of using the APIs.

## The video stack provided by the Raspberry Foundation

Sometime ago, the Raspberry PI foundation [declared the historical camera APIs as legacy](https://www.raspberrypi.com/documentation/computers/camera_software.html#libcamera-and-libcamera-apps). In some `Raspbian OS` images there is still the possibility to switch the camera stack to the legacy mode and use the support (`VideoDevice` class) that we had for years in the `dotnet/iot` library.

Since the transition to `libcamera`, the APIs are now very different. While in theory this is a great news, the decision from the `libcamera` team to only provide a `C++ API` without any `C language` wrapper makes the interoperability with any language very difficult.

> The `C++ language`, does not provide a stable binary interface (**`ABI`**) over time. This means that if you use a different compiler brand or even just a different compiler version, the binaries may be different. The consequence is breaking the compatibility and making the caller application crash.
>
> To avoid this problem, you have to recompile the C++ library from the sources every time. Anyway, recompiling all of the `libcamera` source code in the `dotnet/iot` pipeline would heavily affect the performance of the CI/CD process and it was a unanimous decision to avoid that.
>
> There is also an additional issue with recompiling from the sources. Should we do it, we should also maintain all the different version with different binaries because we do not have control over the `libcamera` version shipped (or upgraded) in the `Raspbian OS`.

This problem is not something limited to the `.NET` world, but any language. For example [this Rust library](https://github.com/lit-robotics/libcamera-rs) have the same issues and created a `C API` wrapper for their own use. C++ is a great language choice, but unfortunately it makes the interoperability with other languages extremely difficult.

### LibCamera Compatibility Layer

The `libcamera` API provides a sort of compatibility layer which aimed to expose an API compatible with the legacy API. But [the tests I did](https://github.com/dotnet/iot/issues/1875#issuecomment-1156406558) proved that this is not a viable solution. The compatibility layer did not reach a maturity level sufficient to provide a stable and working solution.

### Writing a C Wrapper

Since `libcamera` does not provide a `C language` wrapper around the `C++ library`, this can be written separately from anyone. While this is a possible solution, there is still the problem to maintain aligned the `C wrapper` with the `C++ compilation` which ends up in the same nightmare of interfacing directly C# with the C++ [mangled names](https://en.wikipedia.org/wiki/Name_mangling).

Also, since `libcamera` has not reached yet a stable release, the API shape could still change at any moment in time making this task more difficult.

### Why is the Python wrapper successful

The Raspberry Foundation successfully provides a Python library. This may raise the question why we could not do the same. They could do it more easily because they ship together the Raspbian Operating System, the `libcamera` library and the wrapper, making the version alignment straightforward.

On our side, we have no visibility over the upcoming API/ABI changes and we would need to maintain several versions to support current and past versions of the library shipped by the OS or available as an upgrade (using `apt-get`). This makes the task harder for both the maintainers (`dotnet/iot`) and the users which should download the matching binary version every time.

### H.264 hardware encoder

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
