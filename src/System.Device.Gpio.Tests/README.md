# How to run the tests
This shows how to run the tests on a Raspberry Pi. On other platforms, things should be similar. 

## Building on the desktop PC
(to be extended)

## Building directly on the Pi
Prerequisites: 
- Installed .NET SDK with the "dotnet" executable in the path
- Locally cloned repository

Build System.Device.Gpio.dll:
```
cd src/System.Device.Gpio
dotnet build System.Device.Gpio.sln
```
This builds the main System.Device.Gpio assembly together with its test assembly. Before running the tests, you need to:
- Connect BCM Pins 12 and 16 (physical Pins 32 and 36) with a cable. It is suggested to add a resistor between 1kΩ and 10kΩ between the pins, this protects the PI in the case of a missconfiguration (i.e both pins set to Out, one high, the other low). 

## Raspberry Pi driver tests
After that, you can run the tests with the RaspberryPiDriver (which is the default low-level driver for the Raspberry Pi) like:
```
dotnet test --filter RaspberryPiDriverTests System.Device.Gpio.sln 
```
Depending on the version of the Pi and the installed Linux distribution, it may be required to run the tests as root:
```
sudo dotnet test --filter RaspberryPiDriverTests System.Device.Gpio.sln 
```

If everything went smoothly, the output should end with a success message. 

## SysFsDriver Tests
You can also run the SysFsDriver Tests (this uses a more generic approach). This driver requires root permissions to run, so you need to `sudo` the command:
```
sudo dotnet test --filter SysFsDriverTests System.Device.Gpio.sln 
```

## LibgpiodDriver Tests
To run the Libgpiod Driver test (a Linux Kernel Driver for the GPIO device), you need to first install the libgpiod package:
```
sudo apt install -y libgpiod-dev
```
These tests do not require root permissions to run:
```
dotnet test --filter LibGpiodDriverTests System.Device.Gpio.sln 
```
