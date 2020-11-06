# HC-SR501 - Samples

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
GpioController ledController = new GpioController();
ledController.OpenPin(27, PinMode.Output);

using (Iot.Device.Hcsr501.Hcsr501 sensor =
    new Iot.Device.Hcsr501.Hcsr501(17))
{
    while (true)
    {
        // adjusting the detection distance and time by rotating the potentiometer on the sensor
        if (sensor.IsMotionDetected)
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

        Thread.Sleep(1000);
    }
}
```



## Result
![](RunningResult.gif)
