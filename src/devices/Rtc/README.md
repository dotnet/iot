# Realtime Clock

The RTC devices supported by the project include DS1307, DS3231, PCF8563.

A class for setting the system clock is also provided, so that for instance the Raspberry Pi's operating system time can be synchronized
to a hardware RTC clock.

## Documentation

- DS1307 [datasheet](https://www.analog.com/media/en/technical-documentation/data-sheets/DS1307.pdf)
- DS3231 [datasheet](https://datasheets.maximintegrated.com/en/ds/DS3231.pdf)
- PCF8563 [datasheet](https://www.nxp.com/docs/en/data-sheet/PCF8563.pdf)

## Board

![Circuit DS1307](Circuit_DS1307_bb.png)

## Usage with Hardware clocks

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

## Setting the operating system clock

The class `SystemClock` contains static methods to set the operating system clock. Since this operation requires elevated permissions,
some special requirements apply unless the application is run as root (on Linux or MacOs)/administrator (on Windows).

On linux or MacOs, the user calling the `SetSystemTimeUtc` must either be root or the `date` command must have the setUid bit set. To do this, one must execute this command once: `sudo chmod +s /bin/date`. This allows everyone to set the clock.

On Windows, a system policy exists to allow anybody of a named user group to set the system clock. Normally, this right is limited to users belonging to the "Administrators" group. To configure it, open 'gpedit.msc' and go to Computer Configuration > Windows Settings > Security Settings > Local Policies > User Rights Assignments and add the user or his group to the setting 'Change System Time'.
