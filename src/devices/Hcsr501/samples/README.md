# Example of HC-SR501

## Hardware Required
* PIR Motion Sensor - HC-SR501
* LED
* 220 Ω resistor
* Male/Female Jumper Wires

## Circuit
![](circuit_bb.png)

### HC-SR501

![](Hcsr501Setting.png)

* VCC - 5V
* GND - GND
* OUT - GPIO 17

### LED
* VCC & 220 Ω resistor - GPIO 27
* GND - GND

## Code
```C#
GpioController ledController = new GpioController(PinNumberingScheme.Logical);
// open PIN 27 for led
ledController.OpenPin(27, PinMode.Output);

using(Hcsr501 sensor = Hcsr501(17, PinNumberingScheme.Logical))
{
    // loop
    while (true)
    {
        // Adjusting the detection distance and time by rotating the potentiometer on the sensor
        // For more information, you can see the picture above or the datasheet in src/devices/Hcsr501/README.md
        if (sensor.IsMotionDetected == true)
        {
            // turn the led on when the sensor detected infrared heat
            ledController.Write(27, PinValue.High);
            Console.WriteLine("Detected! Turn the LED on.");
        }
        else
        {
            // turn the led off when the sensor undetected infrared heat
            ledController.Write(27, PinValue.Low);
            Console.WriteLine("Undetected! Turn the LED off.");
        }

        // wait for a second
        Thread.Sleep(1000);
    }
}
```



## Result
![](res.gif)
