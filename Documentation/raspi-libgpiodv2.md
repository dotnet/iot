# Use libgpiodv2 on a Raspberry Pi

If you want to control GPIOs using libgpiod version 2 the library must be installed.

As of now (12/2023) only a few distros ship version 2 with their package manager, which requires manual installation.

## Install libgpiod v2.x

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



# GPIO blink example using libgpiodv2

If not already done, install dotnet, see "Install dotnet" below.

`dotnet iot` will search common library paths on Linux to find `libgpiod.so.3` so no step is required here.

Simply use the `GpioController` like this:

``````c#
// side note: on the Raspberry Pi the GPIO chip line offsets are the same numbers as the usual BCM GPIO numbering, which is convenient
const int inputPin = 14;
const int outputPin = 15;

// on the Pi3,4 you most likely want 0, on the Pi5 number 4, see 'gpioinfo' tool
const int chipNumber = 0;
// 'using' will dispose the controller when it falls out of scope, which will un-claim lines
using var gpioController = new GpioController(chipNumber);

gpioController.OpenPin(inputPin);
gpioController.OpenPin(outputPin);

for (int i = 0; i < 5; i++)
{
    controller.Write(outputPin, PinValue.High);
    await Task.Delay(1000);
    controller.Write(outputPin, PinValue.Low);
    await Task.Delay(1000);
}
``````

Or register a callback for edge events:

``````c#
...
controller.RegisterCallbackForPinValueChangedEvent(inputPin, PinEventTypes.Falling | PinEventTypes.Rising, (sender, eventArgs) =>
{
    int pinNr = eventArgs.PinNumber;
    PinEventTypes changeType = eventArgs.ChangeType;
    Console.WriteLine($"Line '{pinNr}' changed to '{changeType}'");
});
...
``````



# Install dotnet

Currently `dotnet iot` targets .NET 6 (might change in the future)

``````shell
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
``````

See [docs](https://learn.microsoft.com/de-de/dotnet/iot/deployment)