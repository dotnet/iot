# ADS1115
ADS1115 is an Analog-to-Digital converter (ADC) with 16 bits of resolution.

## Sensor Image
![](sensor.jpg)

## Usage
* First, you need to create a ADS1115 object. After that you should call Initialize() to initialize.
    ```C#
    Ads1115 adc = new Ads1115(OSPlatform.Linux, 1, AddressSetting.GND, InputMultiplexeConfig.AIN0, PgaConfig.FS6144);
    adc.Initialize();
    ```

* Second, call ReadRaw() to read raw data, and RawToVoltage() to convert raw data to voltage.
    ```C#
    short raw = adc.ReadRaw();
    double voltage = adc.RawToVoltage(raw);
    ```

* If you want to close the sensor, call Dispose().
    ```C#
    adc.Dispose()
    ```

## References
https://github.com/ZhangGaoxing/windows-iot-demo/tree/master/src/ADS1115/01_Datasheet
