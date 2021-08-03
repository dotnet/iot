# Realtime Clock

The RTC devices supported by the project include DS1307, DS3231, PCF8563.

## Documentation

- DS1307 [datasheet](https://cdn.datasheetspdf.com/pdf-down/D/S/1/DS1307-Maxim.pdf)
- DS3231 [datasheet](https://datasheets.maximintegrated.com/en/ds/DS3231.pdf)
- PCF8563 [datasheet](https://cdn.datasheetspdf.com/pdf-down/P/C/F/PCF-856.pdf)

## Board

![Circuit DS1307](Circuit_DS1307_bb.png)

## Usage

### Hardware Required

- DS1307/DS3231/PCF8563
- Male/Female Jumper Wires

```csharp
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

### Result

![Sample result](RunningResult.jpg)
