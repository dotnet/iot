# DS1307 - Samples

## Hardware Required
* DS1307
* Male/Female Jumper Wires

## Circuit
![](Circuit_DS1307_bb.png)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND

## Code
```C#
Console.WriteLine("Hello, Realtime Clock DS1307!");

I2cConnectionSettings settings = new I2cConnectionSettings(1, Ds1307.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Ds1307 rtc = new Ds1307(device))
{
    // set DS1307 time
    rtc.DateTime = DateTime.Now;

    // loop
    while (true)
    {
        // read time
        DateTime dt = rtc.DateTime;

        Console.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
        Console.WriteLine();

        // wait for a second
        Thread.Sleep(1000);
    }
}
```

## Result
![](RunningResult.jpg)
