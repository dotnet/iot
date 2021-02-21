# GPIO Driver for Special Chip

This project contains some **full function(PULL-UP, PULL-DOWN)** generic GPIO drivers, and it can provide faster GPIO access.

## Catalogue

* For Allwinner SoCs: [SunxiDriver](Drivers/Sunxi/README.md)
* For Rockchip SoCs: [RockchipDriver](Drivers/Rockchip/README.md)

## Benchmarks

The test uses different GPIO drivers to quickly switch the state of GPIO, and uses an oscilloscope to measure the average frequency of GPIO externally.

### SunxiDriver

Benchmarking with **Orange Pi Zero**. The operating system is Armbian, Linux kernel version is 5.10.16, and .NET version is 5.0.3.

| Drivers | Library Version | Average Frequency |  |
| :-: | :-: | :-: | :-: |
| SunxiDriver | - | 185 KHz | <img src="imgs/SunxiDriver/sunxi.jpg" height="120"/> |
| SysFsDriver | System.Device.Gpio 1.3.0 | 692 Hz | <img src="imgs/SunxiDriver/sysfs.jpg" height="120"/> |
| LibGpiodDriver | System.Device.Gpio 1.3.0 <br/> libgpiod 1.2-3 | 81 KHz | <img src="imgs/SunxiDriver/libgpiod.jpg" height="120"/> |

### RockchipDriver

Benchmarking with **Orange Pi 4**. The operating system is Armbian, Linux kernel version is 5.10.16, and .NET version is 5.0.3.

| Drivers | Library Version | Average Frequency |  |
| :-: | :-: | :-: | :-: |
| RockchipDriver | - | 426 KHz | <img src="imgs/RockchipDriver/rockchip.jpg" height="120"/> |
| SysFsDriver | System.Device.Gpio 1.3.0 | 3.99 KHz | <img src="imgs/RockchipDriver/sysfs.jpg" height="120"/> |
| LibGpiodDriver | System.Device.Gpio 1.3.0 <br/> libgpiod 1.2-3 | Unable to test due to segment fault | - |