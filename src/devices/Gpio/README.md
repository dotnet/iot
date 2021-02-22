# GPIO Driver for Special Chip

This project contains some **full function(PULL-UP, PULL-DOWN)** generic GPIO drivers, and it can provide faster GPIO access.

## Catalogue

* For Allwinner SoCs: [SunxiDriver](Drivers/Sunxi/README.md)
* For Rockchip SoCs: [RockchipDriver](Drivers/Rockchip/README.md)

## Benchmarks

The test uses different GPIO drivers to quickly switch the state of GPIO, and uses an oscilloscope to measure the average frequency of GPIO externally.

### SunxiDriver

Benchmarking with **Orange Pi Zero**, select GPIO 6 (Logical). The operating system is Armbian buster, Linux kernel version is 5.10.16, and .NET version is 5.0.3.

| Drivers| Language | Library Version | Average Frequency |  |
| :-: | :-: | :-: | :-: | :-: |
| SunxiDriver | C# | - | 185 KHz | <img src="imgs/SunxiDriver/sunxi.jpg" height="120"/> |
| SysFsDriver | C# | System.Device.Gpio 1.3.0 | 692 Hz | <img src="imgs/SunxiDriver/sysfs.jpg" height="120"/> |
| LibGpiodDriver | C# | System.Device.Gpio 1.3.0 <br/> libgpiod 1.2-3 | 81 KHz | <img src="imgs/SunxiDriver/libgpiod.jpg" height="120"/> |
| [wiringOP](https://github.com/orangepi-xunlong/wiringOP) | C | 35de015 | 1.10 MHz | <img src="imgs/SunxiDriver/wiringOP.jpg" height="120"/> |

### RockchipDriver

Benchmarking with **Orange Pi 4**, select GPIO 150 (Logical). The operating system is Armbian bullseye, Linux kernel version is 4.4.213, and .NET version is 5.0.3.

| Drivers| Language | Library Version | Average Frequency |  |
| :-: | :-: | :-: | :-: | :-: |
| RockchipDriver | C# | - | 516 KHz | <img src="imgs/RockchipDriver/rockchip.jpg" height="120"/> |
| SysFsDriver | C# | System.Device.Gpio 1.3.0 | 4.27 KHz | <img src="imgs/RockchipDriver/sysfs.jpg" height="120"/> |
| LibGpiodDriver | C# | System.Device.Gpio 1.3.0 <br/> libgpiod 1.6.2-1 | Unable to test due to segment fault | - |
| [wiringOP](https://github.com/orangepi-xunlong/wiringOP) | C | 35de015 | 584 KHz | <img src="imgs/RockchipDriver/wiringOP.jpg" height="120"/> |