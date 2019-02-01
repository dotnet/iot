# DS3231
DS3231 is a low-cost, extremely accurate I2C realtime clock (RTC) with an integrated temperature compensated crystal oscillator (TCXO) and crystal.

## Sensor Image
![](sensor.jpg)

## Usage
* First, you need to create a DS3231 object. After that you should call Initialize() to initialize.
    ```C#
    Ds3231 rtc = new Ds3231(OSPlatform.Linux);
    rtc.Initialize();
    ```

* After that, call SetTime() to set time
    ```C#
    rtc.SetTime(DateTime.Now);
    ```

* Then use ReadTemperature() to read temperature, ReadTime() to read time
    ```C#
    double temp = rtc.ReadTemperature();
    DateTime dt = rtc.ReadTime();
    ```

* If you want to close the sensor, call Dispose().
    ```C#
    rtc.Dispose()
    ```

## References
https://github.com/ZhangGaoxing/windows-iot-demo/tree/master/src/DS3231/01_Datasheet
