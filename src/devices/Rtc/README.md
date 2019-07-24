# Realtime Clock
The RTC devices supported by the project include DS1307, DS3231, PCF8563.

## Usage
```C#
using Iot.Device.Rtc;

I2cConnectionSettings settings = new I2cConnectionSettings(1, Ds1307.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Ds1307 rtc = new Ds1307(device))
{
    // set Ds1307 time
    rtc.DateTime = DateTime.Now;
    // read time
    DateTime dt = rtc.DateTime;
}
```

## References
DS1307: https://cdn.datasheetspdf.com/pdf-down/D/S/1/DS1307-Maxim.pdf

DS3231: https://datasheets.maximintegrated.com/en/ds/DS3231.pdf

PCF8563: https://cdn.datasheetspdf.com/pdf-down/P/C/F/PCF-856.pdf
