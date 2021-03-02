# GpioDriver for other boards

This project contains some **full function(PULL-UP, PULL-DOWN)** generic GPIO drivers, and it can provide faster GPIO access.

## Catalogue

* For Allwinner SoCs: [SunxiDriver](Drivers/Sunxi/README.md)
* For Rockchip SoCs: [RockchipDriver](Drivers/Rockchip/README.md)

## Benchmarks

The test uses different GPIO drivers to quickly switch the state of GPIO, and uses an oscilloscope to measure the average frequency of GPIO externally.

### SunxiDriver

Benchmarking with **Orange Pi Zero**, select GPIO 6 (Logical). The operating system is Armbian buster, Linux kernel version is 5.10.16, and .NET version is 5.0.3.

| Drivers| Language | Library Version | Test Date | Average Frequency |  |
| :-: | :-: | :-: | :-: | :-: | :-: |
| SunxiDriver | C# | - | 2020-02-20 | 185 KHz | <img src="imgs/SunxiDriver/sunxi.jpg" height="120"/> |
| SysFsDriver | C# | System.Device.Gpio 1.3.0 | 2020-02-20 | 692 Hz | <img src="imgs/SunxiDriver/sysfs.jpg" height="120"/> |
| LibGpiodDriver | C# | System.Device.Gpio 1.3.0 <br/> libgpiod 1.2-3 | 2020-02-20 | 81 KHz | <img src="imgs/SunxiDriver/libgpiod.jpg" height="120"/> |
| [wiringOP](https://github.com/orangepi-xunlong/wiringOP) | C | 35de015 | 2020-02-22 | 1.10 MHz | <img src="imgs/SunxiDriver/wiringOP.jpg" height="120"/> |

### RockchipDriver

Benchmarking with **Orange Pi 4**, select GPIO 150 (Logical). The operating system is Armbian buster, Linux kernel version is 5.10.16, and .NET version is 5.0.3.

| Drivers| Language | Library Version | Test Date | Average Frequency |  |
| :-: | :-: | :-: | :-: | :-: | :-: |
| RockchipDriver | C# | - | 2020-02-22 | 516 KHz | <img src="imgs/RockchipDriver/rockchip.jpg" height="120"/> |
| SysFsDriver | C# | System.Device.Gpio 1.3.0 | 2020-02-22 | 4.27 KHz | <img src="imgs/RockchipDriver/sysfs.jpg" height="120"/> |
| LibGpiodDriver | C# | System.Device.Gpio 1.3.0 <br/> libgpiod 1.2-3 | 2020-02-22 | 174 KHz | <img src="imgs/RockchipDriver/libgpiod.jpg" height="120"/> |
| [wiringOP](https://github.com/orangepi-xunlong/wiringOP) | C | 35de015 | 2020-02-22 | 584 KHz | <img src="imgs/RockchipDriver/wiringOP.jpg" height="120"/> |
