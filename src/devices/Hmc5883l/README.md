# HMC5883L - 3 Axis Digital Compass

HMC5883L is a surface-mount, multi-chip module designed for low-field magnetic sensing with a digital interface for applications such as lowcost compassing and magnetometry.

## Documentation

- HMC5883L [datasheet](https://cdn-shop.adafruit.com/datasheets/HMC5883L_3-Axis_Digital_Compass_IC.pdf)

![sensor](sensor.jpg)

## Usage

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Hmc5883l.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Hmc5883l sensor = new Hmc5883l(device))
{
    // read direction vector
    Vector3 directionVector = sensor.DirectionVector;
    // read heading
    double heading = sensor.Heading;
    // read status
    Status status = sensor.DeviceStatus;
}

```

From the [HMC5883L sample](https://github.com/dotnet/iot/tree/main/src/devices/Hmc5883l/samples), you can go further with the following:

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Hmc5883l.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Hmc5883l sensor = new Hmc5883l(device))
{
    while (true)
    {
        // read heading
        Console.WriteLine($"Heading: {sensor.Heading.ToString("0.00")} °");
        Console.WriteLine();

        // wait for a second
        Thread.Sleep(1000);
    }
}

```

### Hardware Required

- HMC5883L
- Male/Female Jumper Wires

### Circuit

![circuit](./HMC5883L_circuit_bb.png)

- SCL - SCL
- SDA - SDA
- VCC - 5V
- GND - GND

### Result

![running result](./RunningResult.jpg)
