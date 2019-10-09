# How to run tests on the Raspberry Pi

Prerequisites: 
- Installed .NET SDK with the "dotnet" executable in the path
- Locally cloned repository

Build System.Device.Gpio.dll:
```
cd src/System.Device.Gpio
dotnet build System.Device.Gpio.sln
```
This builds the main System.Device.Gpio assembly together with its test assembly. Before running the tests, you need to:
- Connect an LED in series with a matching resistor to BCM Pin 18 (physical Pin 12) 
- Connect BCM Pins 12 and 16 (physical Pins 32 and 36) with a cable

## Raspberry Pi driver tests
After that, you can run the tests with the RaspberryPiDriver (which is the default low-level driver for the Raspberry Pi) like:
```
dotnet test --filter RaspberryPiDriverTests System.Device.Gpio.sln 
```
During the tests, you should see the LED light up for a second. 

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
