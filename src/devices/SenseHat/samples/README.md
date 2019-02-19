# Sense HAT

## LED matrix

```csharp
using (var m = new SenseHatLedMatrixI2c())
{
    m.Clear(Color.Purple);
    m.SetPixel(0, 0, Color.Red);
    m.SetPixel(1, 0, Color.Green);
    m.SetPixel(2, 0, Color.Blue);

    for (int i = 1; i <= 7; i++)
    {
        m.SetPixel(i, i, Color.White);
    }
}
```

Please see [sample](LedMatrix.Sample.cs) for more advanced example (also includes above).

There are currently two implementations of SenseHatLedMatrix:
- SenseHatLedMatrixSysFs - uses native driver - requires installing a driver but colors look (arguably) more vivid (also a bit faster).
- SenseHatLedMatrixI2c - does not require any drivers - allows using brighter colors but colors are more pale by default.

Both implementations implement SenseHatLedMatrix abstraction which should be used in the code.

## Joystick

```csharp
using (var j = new SenseHatJoystick())
{
    while (true)
    {
        j.Read();
        Console.Clear();
        if (j.HoldingUp)
            Console.Write("U");
        if (j.HoldingDown)
            Console.Write("D");
        if (j.HoldingLeft)
            Console.Write("L");
        if (j.HoldingRight)
            Console.Write("R");
        if (j.HoldingButton)
            Console.Write("!");
    }
}
```

## Accelerometer and Gyroscope

```csharp
using (var ag = new SenseHatAccelerometerAndGyroscope())
{
    while (true)
    {
        Console.WriteLine($"Acceleration={ag.Acceleration}"); 
        Console.WriteLine($"AngularRate={ag.AngularRate}");
        Thread.Sleep(100);
    }
}
```

## Magnetometer

```csharp
using (var m = new SenseHatMagnetometer())
using (var d = new SenseHatLedMatrixI2c())
{
    Console.WriteLine("Move SenseHat around in every direction until dot on the LED matrix stabilizes when not moving.");
    d.Clear();
    Stopwatch sw = Stopwatch.StartNew();
    Vector3 min = m.MagneticInduction;
    Vector3 max = m.MagneticInduction;

    while (min == max)
    {
        Vector3 sample = m.MagneticInduction;
        min = Vector3.Min(min, sample);
        max = Vector3.Max(max, sample);
        Thread.Sleep(50);
    }

    const int intervals = 8;
    Color[] data = new Color[64];
    while (true)
    {
        Vector3 sample = m.MagneticInduction;
        min = Vector3.Min(min, sample);
        max = Vector3.Max(max, sample);
        Vector3 size = max - min;
        Vector3 pos = Vector3.Divide(Vector3.Multiply((sample - min), intervals - 1), size);
        int x = Math.Clamp((int)pos.X, 0, intervals - 1);

        // reverse y to match magnetometer coordinate system
        int y = intervals - 1 - Math.Clamp((int)pos.Y, 0, intervals - 1);
        int idx = SenseHatLedMatrix.PositionToIndex(x, y);

        // fading
        for (int i = 0; i < 64; i++)
        {
            data[i] = Color.FromArgb((byte)Math.Clamp(data[i].R - 1, 0, 255), data[i].G, data[i].B);;
        }

        Color col = data[idx];
        col = Color.FromArgb(Math.Clamp(col.R + 20, 0, 255), col.G, col.B);
        Vector2 pos2 = new Vector2(sample.X, sample.Y);
        Vector2 center2 = Vector2.Multiply(new Vector2(min.X + max.X, min.Y + max.Y), 0.5f);
        float max2 = Math.Max(size.X, size.Y);
        float distFromCenter = (pos2 - center2).Length();
        data[idx] = Color.FromArgb(0, 255, (byte)Math.Clamp(255 * distFromCenter / max2, 0, 255));
        d.Write(data);
        data[idx] = col;
        Thread.Sleep(50);
    }
}
```

## Temperature and humidity

```csharp
using (var th = new SenseHatTemperatureAndHumidity())
{
    while (true)
    {
        Console.WriteLine($"Temperature: {th.Temperature}C   Humidity: {th.Humidity}%rH");
        Thread.Sleep(1000);
    }
}
```
