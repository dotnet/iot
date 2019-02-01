# ADXL345
ADXL345 is a small, thin, low power, 3-axis accelerometer with high resolution (13-bit) measurement at up to ±16g.

## Sensor Image
![](sensor.jpg)

## Usage
* First, you need to create a ADXL345 object. After that you should call Initialize() to initialize.
    ```C#
    // OSPlatform, SPI Bus ID, CS Pin, GravityRange
    Adxl345 sensor = new Adxl345(OSPlatform.Linux, 0, 0, GravityRange.Four);
    sensor.Initialize();
    ```

* Then use ReadAcceleration() to read acceleration.
    ```C#
    Acceleration data = sensor.ReadAcceleration();
    ```

* If you want to close the sensor, call Dispose().
    ```C#
    sensor.Dispose()
    ```

## References
In Chinese : http://wenku.baidu.com/view/87a1cf5c312b3169a451a47e.html

In English : https://github.com/ZhangGaoxing/windows-iot-demo/tree/master/src/ADXL345/01_Datasheet
