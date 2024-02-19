# Control GPIO pins within rootless Docker container on Raspberry Pi

When it comes to controlling the GPIO pins of a Raspberry Pi, the existing tutorials will get you started in a couple of minutes. But most of the code assumes that it runs on bare metal and has sufficient privileges, e. g. to access the GPIO pins.  
But what if you want to run this code in a Docker container, or even rootless? This page will explain how to set up all the necessary pieces.

## Prerequisites

In order to follow the instructions below, Docker must be installed on the Raspberry Pi. The official Docker documentation contains an excellent starting point for installing Docker either for the 32-bit (`armhf`, [see here][5]) or 64-bit (`arm64`, [see here][6]) OS.

## Containerizing the app

Let's use the following sample code to blink an LED ([taken from the official documentation][2]) and save it under `BlinkLedSample\Program.cs`:

```csharp
using System;
using System.Device.Gpio;
using System.Threading;

Console.WriteLine("Blinking LED. Press Ctrl+C to end.");
int pin = 18;
using var controller = new GpioController();
controller.OpenPin(pin, PinMode.Output);
bool ledOn = true;
while (true)
{
    controller.Write(pin, ((ledOn) ? PinValue.High : PinValue.Low));
    Thread.Sleep(1000);
    ledOn = !ledOn;
}
```

The project file `BlinkLedSample\BlinkLedSample.csproj` looks like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="System.Device.Gpio" Version="3.1.0" />
  </ItemGroup>
</Project>
```

As you can see, it is a .NET 8 console app which references the NuGet package `System.Device.Gpio`.

First of all, let's make sure that this compiles nicely by running `dotnet build`:

```shell
MSBuild version 17.8.3+195e7f5a3 for .NET
  Determining projects to restore...
  Restored C:\work\temp\BlinkLedSample\BlinkLedSample.csproj (in 125 ms).
  BlinkLedSample -> C:\work\temp\BlinkLedSample\bin\Debug\net8.0\BlinkLedSample.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:00.88
```

The .NET SDK has built-in support for containerizing a .NET app ([see here][3]). All we have to do is to add a NuGet package reference to `Microsoft.NET.Build.Containers` so that the necessary build components are available (note: this step won't be necessary from .NET `8.0.200` on or when working with ASP.NET Core):

```shell
dotnet add package Microsoft.NET.Build.Containers
```

Now we can create a Docker image of our app by calling `dotnet publish --os linux --arch arm64 /t:PublishContainer -c Release`:

```shell
MSBuild version 17.8.3+195e7f5a3 for .NET
  Determining projects to restore...
  Restored C:\work\temp\BlinkLedSample\BlinkLedSample.csproj (in 115 ms).
  BlinkLedSample -> C:\work\temp\BlinkLedSample\bin\Release\net8.0\linux-arm64\BlinkLedSample.dll
  BlinkLedSample -> C:\work\temp\BlinkLedSample\bin\Release\net8.0\linux-arm64\publish\
  Building image 'blinkledsample' with tags 'latest' on top of base image 'mcr.microsoft.com/dotnet/runtime:8.0'.
  Pushed image 'blinkledsample:latest' to local registry via 'docker'.
```

This does the following:

- `dotnet publish` is instructed to publish a Docker image which targets Linux and Raspi's `arm64` architecture.
- The project is restored and built using the `Release` configuration.
- The Docker base image `mcr.microsoft.com/dotnet/runtime:8.0` is resolved automatically by taking the type of app (console, ASP.NET Core, worker, etc.) and target framework (`7.0`, `8.0`, etc.) into account.
- The Docker image of our app is built, using the `latest` tag by default, i. e. the local Docker registry now contains the image `blinkledsample:latest`.

This image can now be transferred onto the Raspberry Pi, e. g. via using a Docker registry like Docker Hub or `docker save` and `docker load`.

## Running the app in Docker

Assuming we're on a Raspberry Pi, we could now create a container from the previously created image like this:

```shell
docker run --rm --name blinkled blinkledsample:latest
```

However, this will fail as the app has no access to the GPIO pins inside the container. As with every other resource necessary for running a container, it must be mounted from the host into the container. In case of GPIO, the device `/dev/gpiomem` is needed.  
The complete command to run the container therefore is:

```shell
docker run --rm --device=/dev/gpiomem --name blinkled blinkledsample:latest
```

But executing the command leads to the following error:

```text
Unhandled exception. System.IO.IOException: Error 13 initializing the Gpio driver.
   at System.Device.Gpio.Drivers.RaspberryPi3LinuxDriver.Initialize()
   at System.Device.Gpio.Drivers.RaspberryPi3LinuxDriver.OpenPin(Int32 pinNumber)
   at System.Device.Gpio.Drivers.RaspberryPi3Driver.OpenPin(Int32 pinNumber)
   at System.Device.Gpio.GpioController.OpenPinCore(Int32 pinNumber)
   at System.Device.Gpio.GpioController.OpenPin(Int32 pinNumber)
   at System.Device.Gpio.GpioController.OpenPin(Int32 pinNumber, PinMode mode)
```

This piece is a bit more tricky: first of all, let's check the permissions inside the container for the device:

```shell
myUser@myRaspi:~ $ docker run -it --rm --entrypoint /bin/bash blinkledsample:latest
app@18c678710307:/app$ ls -lh /dev/gpiomem
crw-rw---- 1 root 997 245, 0 Jan 28 18:45 /dev/gpiomem
```

Now we know the following about `/dev/gpiomem`:

- It is user-owned by `root` with read and write permissions.
- It is group-owned by the group with ID `997` with read and write permissions.
- All others have no access.

So let's test if our container can access the device:

```shell
app@18c678710307:/app$ test -r /dev/gpiomem; echo "$?"
1
app@18c678710307:/app$ test -w /dev/gpiomem; echo "$?"
1
app@18c678710307:/app$ id
uid=1654(app) gid=1654(app) groups=1654(app)
```

We see that the current user `app` can neither read nor write `/dev/gpiomem`, and that the current user  has the group ID `gid=1654`.  
That perfectly explains the error we're seeing in our app: our app runs as user `app` which belongs to the group `1654`, but only members of the group `997` can access the GPIO device.

Luckily, the desired group ID can be set when running a Docker container:

```shell
docker run --rm -u "1654:997" --device=/dev/gpiomem --name blinkled blinkledsample:latest
```

Now you should see the console output `Blinking LED. Press Ctrl+C to end.` and a blinking LED - congratulations! 🎉

## `root` vs. `app`

Starting with .NET 8, the Docker images for Linux come with a non-root user. There is [this excellent blog article][1] about the topic, but it's worth spending a word on the rootless nature of the described containerization.

Without configuring anything manually, the created Docker image uses the new rootless `app` user. That happened automatically since we used the .NET SDK Container Building Tools.  

In case of using a dedicated `Dockerfile` and building the image via `docker build`, the following line would have to be added:

```dockerfile
USER $APP_UID
```

This way, the user ID can be controlled via the environment variable `APP_UID` and uses `1654` by default.  
Not adding the line and omit using `-u "1654:997"` when starting the Docker container will result in using the `root` user, i. e. there will still be a blinking LED, but the app will have more privileges due to the root permissions.

## Additional information

The following issues and articles provide more in-depth details:

- dotnet/iot#2169
- [Secure your .NET cloud apps with rootless Linux Containers][1]
- [Blink an LED][2]
- [Containerize a .NET app with dotnet publish][3]
- [RaspiFanController][4]

[1]: https://devblogs.microsoft.com/dotnet/securing-containers-with-rootless
[2]: https://learn.microsoft.com/en-us/dotnet/iot/tutorials/blink-led
[3]: https://learn.microsoft.com/en-us/dotnet/core/docker/publish-as-container
[4]: https://github.com/mu88/RaspiFanController
[5]: https://docs.docker.com/engine/install/raspberry-pi-os/
[6]: https://docs.docker.com/engine/install/debian/
