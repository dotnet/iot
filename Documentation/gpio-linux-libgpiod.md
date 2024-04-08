# Using libgpiod to control GPIOs

## Quick usage: blink LED example

This example targets a RaspberryPi 3/4, see comments for more information:

```c#
// side note: on the Raspberry Pi the GPIO chip line offsets are the same numbers as the usual BCM GPIO numbering, which is convenient
const int ledGpio = 15;

// on the Pi3,4 you most likely want 0, on the Pi5 number 4, see 'gpioinfo' tool
const int chipNumber = 0;
// 'using' will dispose the controller when it falls out of scope, which will un-claim lines

// alternatively be more explicit: 'new GpioController(chipNumber, new LibGpiodDriver())'
using var gpioController = new GpioController(chipNumber);

gpioController.OpenPin(ledGpio);

for (int i = 0; i < 5; i++)
{
    controller.Write(ledGpio, PinValue.High);
    await Task.Delay(1000);
    controller.Write(ledGpio, PinValue.Low);
    await Task.Delay(1000);
}
```

## libgpiod versions

**Note**: The documented version of libgpiod is not the same as the library so name, see the following table:

| Documented version | Library so name   | Comment                                                  |
| ------------------ | ----------------- | -------------------------------------------------------- |
| 1.0.2              | libgpiod.so.1.0.2 | last .so.1.x library                                     |
| 1.1                | libgpiod.so.2.0.0 | first occurrence of inconsistency, first .so.2.x library |
| 1.6.4              | libgpiod.so.2.2.2 | last .so.2.x library                                     |
| 2.0                | libgpiod.so.3.0.0 | first .so.3.x library                                    |
| 2.1                | libgpiod.so.3.1.0 | latest .so.3.x library (currently)                       |

## libgpiod version support

Currently (12/23) dotnet-iot supports v0, v1 and v2 of libgpiod.

The following table shows which driver supports which library version

| LibGpiodDriverVersion | Libgpiod version (documented) |
| --------------------- | ----------------------------- |
| V1                    | 0.x to 1.x                    |
| V2                    | 2.x                           |

## Choose LibGpiodDriver Version

If you want to explicitly select the version of the libgpiod driver, to target a specific library version, there are following options:

1. constructor of LibGpiodDriver:

   ```c#
   new LibGpiodDriver(chipNumber, LibGpiodDriverVersion.V1)
   ```

2. Environment variable:

   ```shell
   export DOTNET_IOT_LIBGPIOD_DRIVER_VERSION=V1 // or V2...
   ```

When not explicitly specified, dotnet iot automatically tries to find a driver compatible to what library version is installed.

## Install libgpiod

If you want to control GPIOs using libgpiod, the library must be installed.

Many package managers provide a libgpiod package, for example:

   ```shell
   apt install libgpiod2
   ```

## Install libgpiod manually

The installation should be the same on all Pi's, or boards whose distro uses the APT package manager.

1. Install build dependencies

   ```shell
   sudo apt update && sudo apt install -y autogen autoconf autoconf-archive libtool libtool-bin pkg-config build-essential
   ```

2. Download the tarball and unpack it, see [releases](https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/refs/), e.g.

   ```shell
   wget https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/snapshot/libgpiod-2.1.tar.gz
   tar -xzf libgpiod-2.1.tar.gz
   ```

3. Compile and install (see [docs](https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/))

   ```shell
   cd libgpiod-2.1/
   ./autogen.sh
   make
   sudo make install
   sudo ldconfig
   ```

This will install the library .so files to `/usr/lib/local`

If you want to also build command line utilities `gpioinfo, gpiodetect` etc., specify `./autogen.sh --enable-tools=yes`
