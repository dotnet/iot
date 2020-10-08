# Enabling Hardware PWM on Raspberry Pi

If you want to use the hardware PWM capabilities of your Raspberry Pi, you will need to activate this feature. And if you want to run your code without elevated root privileged to access them, then, you'll as well need to make couple of modifications. This tutorial is her to help you activating the PWM and making sure you'll get the right permissions.

## Basic Hardware PWM code

The most simplest code you can build to use hardware PWM is the following:

```csharp
using System;
using System.Device.Pwm;

namespace TestTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello PWM!");
            var pwm = PwmChannel.Create(0, 0, 400, 0.5);
            pwm.Start();
            Console.ReadKey();
        }
    }
}
```

In this case ```PwmChannel.Create(0, 0, 400, 0.5)``` will create a hardware PWM chip 0 and PWM channel 0 with a frequency of 400 Hz and a duty time of 50%.

When you run the code, if you attached a simple led and a resistor on the physical pin 32, logical 12, you will see a led that will be half bright compare to the same led plugged on a 3.3V pin.

If you get an error message like this one, it means the hardware PWM has not been properly enabled:

```
Hello PWM!
Unhandled exception. System.ArgumentException: The chip number 0 is invalid or is not enabled.
   at System.Device.Pwm.Channels.UnixPwmChannel.Validate()
   at System.Device.Pwm.Channels.UnixPwmChannel..ctor(Int32 chip, Int32 channel, Int32 frequency, Double dutyCycle)
   at System.Device.Pwm.PwmChannel.Create(Int32 chip, Int32 channel, Int32 frequency, Double dutyCyclePercentage)
   at TestTest.Program.Main(String[] args) in C:\tmp\TestTest\TestTest\Program.cs:line 12
Aborted
```

If you get an error message like the following one, it means that you don't have the permission, see the specific section below for this as well:

```
Unhandled exception. System.UnauthorizedAccessException: Access to the path '/sys/class/pwm/pwmchip0/export' is denied.
 ---> System.IO.IOException: Permission denied
```

## Enabling hardware PWM

In order to have the hardware PWM activated on the Raspberry Pi, you'll have to edit the /boot/config.txt file and add an overlay.

Main Raspberry Pi kernel documentation gives 2 possibilities. Either a [single channel](https://github.com/raspberrypi/linux/blob/04c8e47067d4873c584395e5cb260b4f170a99ea/arch/arm/boot/dts/overlays/README#L925), either a [dual channel](https://github.com/raspberrypi/linux/blob/04c8e47067d4873c584395e5cb260b4f170a99ea/arch/arm/boot/dts/overlays/README#L944).

Here are the possible options for each PWM channel:

| PWM | GPIO | Function | Alt | Exposed |
| PWM0 | 12 | 4 | Alt0 | Yes |
| PWM0 | 18 | 2 | Alt5 | Yes |
| PWM0 | 40 | 4 | Alt0 | No |
| PWM0 | 52 | 5 | Alt1 | No |
| PWM1 | 13 | 4 | Alt0 | Yes |
| PWM1 | 19 | 2 | Alt5 | Yes |
| PWM1 | 41 | 4 | Alt0 | No |
| PWM1 | 45 | 4 | Alt0 | No |
| PWM1 | 53 | 5 | Alt1 | No |

Only accessible pin from this list on the Raspberry Pi pin out are GPIO 12, 18, 13 and 19. The other GPIO are not exposed.

### Activating only 1 channel

We have then 4 options for the exposed GPIO pins:

| PWM | GPIO | Function | Alt | dtoverlay |
| PWM0 | 12 | 4 | Alt0 | dtoverlay=pwm,pin=12,func=4 |
| PWM0 | 18 | 2 | Alt5 | dtoverlay=pwm,pin=18,func=2 |
| PWM1 | 13 | 4 | Alt0 | dtoverlay=pwm,pin=13,func=4 |
| PWM1 | 19 | 2 | Alt5 | dtoverlay=pwm,pin=19,func=2 |

Edit the /boot/config.txt file and add the dtoverlay line in the file. You need root privileges for this:

```bash
sudo nano /boot/config.txt
```

Save the file with `ctrl + x` then `Y` then `enter`

Then reboot:

```bash
sudo reboot
```

You are all setup, the basic example should now work with the PWM and channel you have selected.

### Activating 2 channels

| PWM0 | PWM0 GPIO | PWM0 Function | PWM0 Alt |  PWM1 | PWM1 GPIO | PWM1 Function | PWM1 Alt | dtoverlay |
| PWM0 | 12 | 4 | Alt0 | PWM1 | 13 | 4 | Alt0 | dtoverlay=pwm-2chan,pin=12,func=4,pin2=13,func2=4 |
| PWM0 | 18 | 2 | Alt5 | PWM1 | 13 | 4 | Alt0 | dtoverlay=pwm-2chan,pin=18,func=2,pin2=13,func2=4 |
| PWM0 | 12 | 4 | Alt0 | PWM1 | 19 | 2 | Alt5 | dtoverlay=pwm-2chan,pin=12,func=4,pin2=19,func2=2 |
| PWM0 | 18 | 2 | Alt5 | PWM1 | 19 | 2 | Alt5 | dtoverlay=pwm-2chan,pin=18,func=2,pin2=19,func2=2 |

Edit the /boot/config.txt file and add the dtoverlay line in the file. You need root privileges for this:

```bash
sudo nano /boot/config.txt
```

Save the file with `ctrl + x` then `Y` then `enter`

Then reboot:

```bash
sudo reboot
```

You are all setup, the basic example should now work with the PWM and channel you have selected.

## Solving permission issues

When running the basic code, you may have a lack of permissions:

```
Unhandled exception. System.UnauthorizedAccessException: Access to the path '/sys/class/pwm/pwmchip0/export' is denied.
 ---> System.IO.IOException: Permission denied
   --- End of inner exception stack trace ---
   at Interop.ThrowExceptionForIoErrno(ErrorInfo errorInfo, String path, Boolean isDirectory, Func`2 errorRewriter)
   at Microsoft.Win32.SafeHandles.SafeFileHandle.Open(String path, OpenFlags flags, Int32 mode)
   at System.IO.FileStream.OpenHandle(FileMode mode, FileShare share, FileOptions options)
   at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share, Int32 bufferSize, FileOptions options)
   at System.IO.StreamWriter.ValidateArgsAndOpenPath(String path, Boolean append, Encoding encoding, Int32 bufferSize)
   at System.IO.StreamWriter..ctor(String path)
   at System.IO.File.WriteAllText(String path, String contents)
   at System.Device.Pwm.Channels.UnixPwmChannel.Open()
   at System.Device.Pwm.Channels.UnixPwmChannel..ctor(Int32 chip, Int32 channel, Int32 frequency, Double dutyCycle)
   at System.Device.Pwm.PwmChannel.Create(Int32 chip, Int32 channel, Int32 frequency, Double dutyCyclePercentage)
   at TestTest.Program.Main(String[] args) in C:\tmp\TestTest\TestTest\Program.cs:line 12
Aborted
```

You have 2 options: running the code with root privileges or adding your user to the pwm group.

### Running code with root privileges

This is straight forward, you have to use ```sudo``` to run your application. Let's say your application is names ```yourapplication```, once in the same directory as your application, it will then be:

```bash
sudo ./yourapplication
```

### Adding your user to the right permission group

If you're running, or just upgraded to a version published after August 2020, this should be already done. 
You will have to create a [specific group in udev](https://raspberrypi.stackexchange.com/questions/66890/accessing-pwm-module-without-root-permissions).

```bash
sudo nano /etc/udev/rules.d/99-com.rules
```

Add the following lines:

```
SUBSYSTEM=="pwm*", PROGRAM="/bin/sh -c '\
        chown -R root:gpio /sys/class/pwm && chmod -R 770 /sys/class/pwm;\
        chown -R root:gpio /sys/devices/platform/soc/*.pwm/pwm/pwmchip* && chmod -R 770 /sys/devices/platform/soc/*.pwm/pwm/pwmchip*\
'"
```

Save the file with `ctrl + x` then `Y` then `enter`

Then reboot:

```bash
sudo reboot
```

You are all setup, the basic example should now work with the PWM and channel you have selected.
