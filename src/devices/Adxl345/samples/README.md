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
```C#
// SPI bus 0
// CS Pin connect to CS0(Pin24)
// set gravity measurement range ±4G
using (Adxl345 sensor = new Adxl345(0, 0, GravityRange.Range2))
{
    // loop
    while (true)
    {
        // read data
        Vector3 data = sensor.Acceleration;

        Console.WriteLine($"X: {data.X.ToString("0.00")} g");
        Console.WriteLine($"Y: {data.Y.ToString("0.00")} g");
        Console.WriteLine($"Z: {data.Z.ToString("0.00")} g");
        Console.WriteLine();

        // wait for 500ms
        Thread.Sleep(500);
    }
}
```

## Result
![](res.jpg)
