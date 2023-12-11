# Using libgpiod to control GPIOs

## Quick usage: blink LED example

This example targets a RaspberryPi 3/4, see comments for more information:

``````c#
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
``````

## Install libgpiod

If you want to control GPIOs using libgpiod, the library must be installed.

Many package managers provide a libgpiod package, for example:

``````shell
apt install libgpiod2
``````

Currently (12/23) dotnet-iot supports v0, v1 and v2 of libgpiod.

**Note**: The documented version of libgpiod is not the same as the library so name, see the following table:

| Documented version | Library so name   | Comment                                                  |
| ------------------ | ----------------- | -------------------------------------------------------- |
| 1.0.2              | libgpiod.so.1.0.2 | last .so.1.x library                                     |
| 1.1                | libgpiod.so.2.0.0 | first occurrence of inconsistency, first .so.2.x library |
| 1.6.4              | libgpiod.so.2.2.2 | last .so.2.x library                                     |
| 2.0                | libgpiod.so.3.0.0 | first .so.3.x library                                    |
| 2.1                | libgpiod.so.3.1.0 | latest .so.3.x library (currently)                       |

## Install libgpiod manually

The installation should be the same on all Pi's, or other boards.

1. Install build dependencies

   ``````shell
   sudo apt update && sudo apt install -y autogen autoconf autoconf-archive libtool libtool-bin pkg-config build-essential
   
   ``````

2. Download the tarball and unpack it: Go to [releases](https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/refs/) and copy the download URL of a libgpiod version

   ``````shell
   wget https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/snapshot/libgpiod-2.1.tar.gz
   tar -xzf libgpiod-2.1.tar.gz
   ``````

   or any other version

3. Compile libgpiod and install (see [BUILDING](https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/))

   ``````shell
   cd libgpiod-2.1/
   ./autogen.sh
   make
   sudo make install
   sudo ldconfig
   ``````

   This will install the library .so files to `/usr/lib/local`

   If you want to also build command line utilities `gpioinfo, gpiodetect` etc., specify `./autogen.sh --enable-tools=yes`
