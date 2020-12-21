# Key Matrix

An M×N key matrix driver.

(M is number of output pins and N is number of input pins.)

## Summary

These key matrices look like this:

![4x4-Keypad](https://www.waveshare.com/img/devkit/accBoard/4x4-Keypad/4x4-Keypad-1.jpg)

This is a 4×4 matrix. And [here is the schematic](https://www.waveshare.com/w/upload/e/ea/4x4-Keypad-Schematic.pdf)

You can connect any M×N key matrix, theoretically, by using M+N GPIO pins.

You can also use any compatible GPIO controller like [Mcp23xxx](../Mcp23xxx) instead of native controller.

## Usage

You need to create 2 lists of int, one for the input and one for the output. 

```csharp
IEnumerable<int> outputs = new int[] { 26, 19, 13, 6 };
IEnumerable<int> inputs = new int[] { 21, 20, 16, 12 };
KeyMatrix mk = new KeyMatrix(outputs, inputs, TimeSpan.FromMilliseconds(20));
```

You can use as well any GpioController like the MCP23017 in the following example

```csharp
var settings = new System.Device.I2c.I2cConnectionSettings(1, 0x20);
var i2cDevice = System.Device.I2c.I2cDevice.Create(settings);
var mcp23017 = new Iot.Device.Mcp23xxx.Mcp23017(i2cDevice);
GpioController gpio = new GpioController(PinNumberingScheme.Logical, mcp23017);
IEnumerable<int> outputs = new int[] { 26, 19, 13, 6 };
IEnumerable<int> inputs = new int[] { 21, 20, 16, 12 };
KeyMatrix mk = new KeyMatrix(outputs, inputs, TimeSpan.FromMilliseconds(20), gpio, true);
```

## Read on Key

To read a key, just the `ReadKey` function:

```csharp
KeyMatrixEvent? key = mk.ReadKey();
```

KeyMatrixEvent contains the event that happened. Please note that ReadKey is blocked up to the moment an event is detected. 

## Event based approach

`KeyMatrix` supports events. Just subscribe to the event and have a function to handle the events:

```csharp
Console.WriteLine("This will now start listening to events and display them. Press a key to finish.");
mk.KeyEvent += KeyMatrixEventReceived;
mk.StartListeningKeyEvent();
while (!Console.KeyAvailable)
{
    Thread.Sleep(1);
}

mk.StopListeningKeyEvent();

void KeyMatrixEventReceived(object sender, KeyMatrixEvent keyMatrixEvent)
{
    // Do something here, you have an event!
}
```

## Tips and tricks

- Using diodes(eg. 1N4148) for each button prevents "ghosting" or "masking" problem.
- Input pins need pull-down resistors connect to ground if your MCU doesn't have it. So you need to have a pull-down on a Raspberry Pi for example.
- If your key matrix doesn't work well, try to swap output and input pins. Some includes diodes and if they are used the reverse way won't work properly.

## References

- http://pcbheaven.com/wikipages/How_Key_Matrices_Works/
- https://www.waveshare.com/wiki/4x4_Keypad
