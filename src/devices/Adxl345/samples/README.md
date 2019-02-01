# Example of ADXL345

## Hardware Required
* ADXL345
* Male/Female Jumper Wires

## Circuit
![](ADXL345_circuit_bb.png)

* VCC - 3.3 V
* GND -  GND
* CS - CS0(Pin24)
* SDO - SPI0 MISO(Pin21)
* SDA - SPI0 MOSI (Pin19)
* SCL - SPI0 SCLK(Pin23)

## Code
* First, you need to create a ADXL345 object. After that you should call Initialize() to initialize.
    ```C#
    // the program runs on Linux
    // SPI bus 0
    // CS Pin connect to CS0(Pin24)
    // set gravity range ±4G
    Adxl345 sensor = new Adxl345(OSPlatform.Linux, 0, 0, GravityRange.Four);
    sensor.Initialize();
    ```

* In the loop, read the sensor data.
    ```C#
    // loop
    while (true)
    {
        // read data
        Acceleration data = sensor.ReadAcceleration();

        Console.WriteLine($"X: {data.X.ToString("0.00")} g");
        Console.WriteLine($"Y: {data.Y.ToString("0.00")} g");
        Console.WriteLine($"Z: {data.Z.ToString("0.00")} g");
        Console.WriteLine();

        // wait for 500ms
        Thread.Sleep(500);
    }
    ```

## Result
![](res.jpg)
