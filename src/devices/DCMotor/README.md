# DC Motor Controller

This is a generic class to control any DC motor.

DC motors are controlled by simply providing voltage on the inputs (inverted voltage inverts the direction).

DC motors can be controlled with 1, 2 or 3 pins.
Please refer to the [sample](./samples/Program.cs) to see how to connect it.

> **Important**: Never connect DC motor directly to your board, instead use i.e. H-bridge

## 3- vs 1/2-pin mode

2/1-pin mode should be used only if H-bridge allows the inputs to be changed frequently
otherwise excessive heat or damage may occur which may reduce life-time of the H-bridge.
It may also cause increased energy consumption due to energy being converted into heat.

## Usage

[See full sample](./samples/Program.cs) for more details.

```csharp
static void Main(string[] args)
{
    const double Period = 10.0;
    Stopwatch sw = Stopwatch.StartNew();
    // 1 pin mode
    // using (DCMotor motor = DCMotor.Create(6))
    // using (DCMotor motor = DCMotor.Create(PwmChannel.Create(0, 0, frequency: 50)))
    // 2 pin mode
    // using (DCMotor motor = DCMotor.Create(27, 22))
    // using (DCMotor motor = DCMotor.Create(new SoftwarePwmChannel(27, frequency: 50), 22))
    // 2 pin mode with BiDirectional Pin
    // using (DCMotor motor = DCMotor.Create(19, 26, null, true, true))
    // using (DCMotor motor = DCMotor.Create(PwmChannel.Create(0, 1, 100, 0.0), 26, null, true, true))
    // 3 pin mode
    // using (DCMotor motor = DCMotor.Create(PwmChannel.Create(0, 0, frequency: 50), 23, 24))
    // Start Stop mode - wrapper with additional methods to disable/enable output regardless of the Speed value
    // using (DCMotorWithStartStop motor = new DCMotorWithStartStop(DCMotor.Create( _any version above_ )))
    using (DCMotor motor = DCMotor.Create(6, 27, 22))
    {
        bool done = false;
        Console.CancelKeyPress += (o, e) =>
        {
            done = true;
            e.Cancel = true;
        };

        string lastSpeedDisp = null;
        while (!done)
        {
            double time = sw.ElapsedMilliseconds / 1000.0;

            // Note: range is from -1 .. 1 (for 1 pin setup 0 .. 1)
            motor.Speed = Math.Sin(2.0 * Math.PI * time / Period);
            string disp = $"Speed = {motor.Speed:0.00}";
            if (disp != lastSpeedDisp)
            {
                lastSpeedDisp = disp;
                Console.WriteLine(disp);
            }

            Thread.Sleep(1);
        }
    }
}
```

![schematics](./dcmotor_bb.png)

![BiDirectional Pin schematics](./DCMotor2pinWithBiDirectionalPin_bb.png)
