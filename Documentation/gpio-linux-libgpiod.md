# Using libgpiod to control GPIOs

## Quick usage: blink LED example

This example targets a Raspberry Pi 3/4, see comments for more information:

```c#
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// On the Raspberry Pi the GPIO chip line offsets match BCM GPIO numbering
const int ledGpio = 15;

// 'using' will dispose the controller when it falls out of scope, which will un-claim lines
// On Pi 3/4 the parameterless constructor auto-selects the best driver
using var controller = new GpioController();

// To explicitly use LibGpiodDriver (e.g. on Pi5 where chip 4 is needed):
// using var controller = new GpioController(new LibGpiodDriver(gpioChip: 4));

controller.OpenPin(ledGpio, PinMode.Output);

for (int i = 0; i < 5; i++)
{
    controller.Write(ledGpio, PinValue.High);
    Thread.Sleep(1000);
    controller.Write(ledGpio, PinValue.Low);
    Thread.Sleep(1000);
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

Dotnet-iot supports v0, v1 and v2 of libgpiod.

The following table shows which driver supports which library version

| .NET IoT Driver | Libgpiod version (documented) |
| --------------- | ----------------------------------------------------- |
| `LibGpiodDriver` | 0.x to 1.0.x (Partial support)  1.1 - 1.x (Supported) |
| `LibGpiodV2Driver` | 2.x |

NOTE: Due to a [breaking change in the values of enums in the libgpiod](
https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/commit/?id=783ff2e3c70788cdd1c65cba9ee0398bda5ebcda), only libgpiod versions 1.1 and later can be expected to function reliably with the V1 driver.
To check what libgpiod packages you have on a deb based system, use: ``` $apt show libgpiod* ```

## Choose the Right Driver Class

.NET IoT provides two separate driver classes for different versions of the libgpiod library:

- **`LibGpiodDriver`** — for libgpiod v1 (library versions 0.x through 1.x)
- **`LibGpiodV2Driver`** — for libgpiod v2 (library versions 2.x)

To use a specific driver:

```c#
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// For libgpiod v1
using var controller = new GpioController(new LibGpiodDriver(gpioChip: 0));

// For libgpiod v2
using var controller = new GpioController(new LibGpiodV2Driver(chipNumber: 0));
```

When not explicitly specified (i.e., when using `new GpioController()`), .NET IoT automatically tries to find a compatible driver for the installed library version.

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
