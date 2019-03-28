# Sense HAT

## Everything together

```csharp
static void Main(string[] args)
{
    using (var sh = new SenseHat())
    {
        int n = 0;
        int x = 3, y = 3;
        while (true)
        {
            Console.Clear();
            (int dx, int dy, bool holding) = JoystickState(sh);
            if (holding)
                n++;
            x = (x + 8 + dx) % 8;
            y = (y + 8 + dy) % 8;
            sh.Fill(n % 2 == 0 ? Color.DarkBlue : Color.DarkRed);
            sh.SetPixel(x, y, Color.Yellow);
            Console.WriteLine($"Temperature: Sensor1: {sh.Temperature.Celsius} °C   Sensor2: {sh.Temperature2.Celsius} °C");
            Console.WriteLine($"Humidity: {sh.Humidity} %rH");
            Console.WriteLine($"Pressure: {sh.Pressure} hPa");
            Console.WriteLine($"Acceleration: {sh.Acceleration} g");
            Console.WriteLine($"Angular rate: {sh.AngularRate} DPS");
            Console.WriteLine($"Magnetic induction: {sh.MagneticInduction} gauss");
            Thread.Sleep(1000);
        }
    }
}

static (int, int, bool) JoystickState(SenseHat sh)
{
    sh.ReadJoystickState();
    int dx = 0;
    int dy = 0;

    if (sh.HoldingUp)
        dy--; // y goes down

    if (sh.HoldingDown)
        dy++;

    if (sh.HoldingLeft)
        dx--;

    if (sh.HoldingRight)
        dx++;

    return (dx, dy, sh.HoldingButton);
}
```

## LED matrix

```csharp
using (var ledMatrix = new SenseHatLedMatrixI2c())
{
    ledMatrix.Fill(Color.Purple);
    ledMatrix.SetPixel(0, 0, Color.Red);
    ledMatrix.SetPixel(1, 0, Color.Green);
    ledMatrix.SetPixel(2, 0, Color.Blue);

    for (int i = 1; i <= 7; i++)
    {
        ledMatrix.SetPixel(i, i, Color.White);
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
using (var magnetometer = new SenseHatMagnetometer())
using (var ledMatrix = new SenseHatLedMatrixI2c())
{
    Console.WriteLine("Move SenseHat around in every direction until dot on the LED matrix stabilizes when not moving.");
    ledMatrix.Fill();
    Stopwatch sw = Stopwatch.StartNew();
    Vector3 min = magnetometer.MagneticInduction;
    Vector3 max = magnetometer.MagneticInduction;
    while (min == max)
    {
        Vector3 sample = magnetometer.MagneticInduction;
        min = Vector3.Min(min, sample);
        max = Vector3.Max(max, sample);
        Thread.Sleep(50);
    }

    const int intervals = 8;
    Color[] data = new Color[64];

    while (true)
    {
        Vector3 sample = magnetometer.MagneticInduction;
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

        ledMatrix.Write(data);
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
        Console.WriteLine($"Temperature: {th.Temperature.Celsius}°C   Humidity: {th.Humidity}%rH");
        Thread.Sleep(1000);
    }
}
```

## Pressure and temperature

```csharp
using (var th = new SenseHatPressureAndTemperature())
{
    while (true)
    {
        Console.WriteLine($"Temperature: {th.Temperature.Celsius}°C   Humidity: {th.Pressure}hPa");
        Thread.Sleep(1000);
    }
}
```

Note: There are more than 1 temperature sensors on SenseHat board
