# HX711: Load Cell amplifier and 24-Bit ADC Module

## Summary

Device binding for the HX711 weight componet. This componet is necessary if you want to read the weight from a load cell, due to the analog signal and too weak emitted.
The Load Cell Amplifier and ADC Module is a small breakout board for the HX711 IC that allows you to easily read load cells to measure weight. 
By connecting the module to your microcontroller you will be able to read the changes in the resistance of the load cell and with some calibration you’ll be able to get very accurate weight measurements. 
This can be handy for creating your own industrial scale, process control, or simple presence detection.

## Documentation

**[HX711](https://homotix_it.e-mind.it/upld/repository/File/hx711.pdf)**
or
**[HX711](https://html.alldatasheet.com/html-pdf/1132222/AVIA/HX711/457/1/HX711.html)**

**[Example for Arduino](https://www.moreware.org/wp/blog/2020/06/09/arduino-come-funziona-la-board-per-celle-di-carico-hx711/)**

## Usage
```C#
int pinDout = 23;
int pinPD_Sck = 24;

using (var controller = new GpioController())
{
    using (var hx711 = new HX711(pinDout, pinPD_Sck, gpioController: controller, shouldDispose: false))
    {
        hx711.PowerUp();
        Console.WriteLine("HX711 is on.");

        Console.WriteLine("Known weight (in grams) currently on the scale:");
        var weightCalibration = int.Parse(Console.ReadLine());

        hx711.StartCalibration(new Mass(weightCalibration, MassUnit.Gram));

        hx711.Tare();
        Console.WriteLine($"Tare set. Value: {hx711.TareValue}gr");

        var weight = hx711.GetWeight();
        Console.WriteLine($"Weight: {weight}gr");

        hx711.PowerDown();
        Console.WriteLine("HX711 is off.");

        _ = Console.ReadLine();
    }
}
```

## Hardware Required
* Load cell
* HX711
* Male/Female Jumper Wires

## Circuit - Scheme
![Fritz diagram](raspberry_hx711_load_cell.png)

* VCC - 5V
* GND - GND
* Pin Dout - GPIO 23
* Pin PD_SCK - GPIO 24

The fritz diagram above depicts how you should wire your RPi in order to run a example program.

## Pinouts HX711
** Analog Side **
| Name              | PCB description |
| ----------------- | --------------- |
| Sensor red wire   | E+              |
| Sensor black wire | E-              |
| Sensor white wire | A-              |
| Sensor green wire | A+              |

** Digital Side **
| Name              | PCB description         |
| ----------------- | ----------------------- |
| GND               | Ground Power Connection |
| DT                | Data IO Connection      |
| SCK               | Serial Clock Input      |
| VCC               | Power Input             |

## Calibration
First of all HX711 need a calibration process because it can be connected to any load cell that has a different range and sensitivity.
To perform it, simply put a known weight on the load cell and start the calibration via `StartCalibration()`.
If you want a more precise calibration, you can do this several times with different weights.

## Notes
Only connection in Channel A is support at the moment.
