# Example of HC-SR501

## Hardware Required
* PIR Motion Sensor - HC-SR501
* LED
* 220 Ω resistor
* Male/Female Jumper Wires

## Circuit
![](circuit_bb.png)

HC-SR501
* VCC - 5V
* GND - GND
* OUT - GPIO 17

LED
* VCC & 220 Ω resistor - GPIO 27
* GND - GND

## Code
* First, you need to create a HCSR501 object. After that you should call Initialize() to initialize.
    ```C#
    HCSR501 sensor = new HCSR501(17);
    sensor.Initialize();
    ```

* Second, initialize LED.
    ```C#
    GpioController controller = new GpioController(PinNumberingScheme.Gpio);
    led = controller.OpenPin(27, PinMode.Output);
    ```

* Finially, in the loop, read the sensor data.
    ```C#
    while (true)
    {
        if (sensor.Read() == true)
        {
            // turn the led on when the sensor detected infrared heat
            led.Write(PinValue.High);
            Console.WriteLine("Detected! Turn the LED on.");
        }
        else
        {
            // turn the led off when the sensor undetected infrared heat
            led.Write(PinValue.Low);
            Console.WriteLine("Undetected! Turn the LED off.");
        }

        // wait for a second
        Thread.Sleep(1000);
    }
    ```

## Result
![](https://raw.githubusercontent.com/ZhangGaoxing/dotnet-core-iot-demo/master/src/PIR/02_Image/res.gif)