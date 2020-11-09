# HuskyLens

HuskyLens is an easy-to-use AI machine vision sensor with 6 built-in functions: face recognition, object tracking, object recognition, line following, color detection and tag detection.

Through the UART / I2C port, HuskyLens can connect to Arduino, Raspberry Pi, or micro:bit to help you make very creative projects without playing with complex algorithms.

## Information:

Wiki: https://wiki.dfrobot.com/HUSKYLENS_V1.0_SKU_SEN0305_SEN0336

Protocol description: https://github.com/HuskyLens/HUSKYLENSArduino/blob/master/HUSKYLENS%20Protocol.md

## Usage:

Currently only serial port communication is implemented, but `IBinaryConnection` can be implemented for I2C at a later stage.

Note that the serial port presented by the built-in USB connector cannot be used for sending commands, its purpose seems to be restricted to updating firmware. Commands can only be sent to the UART via the JST connector in the bottom left of the device. See the Wiki link above for more information.

First, connect the device to the serial port:
``` csharp
var sp = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One); // using an FTD1232
sp.Open();
var device = new HuskyLens(new SerialPortConnection(sp));
```
Then, check connectivity, set alogrithm and read recognized objects:
``` csharp
if (!device.Ping())
{
    Console.WriteLine("Failed to verify connection with HuskyLens device");
}

Console.WriteLine("Switching to object tracking");
device.Algorithm = Algorithm.ObjectTracking;

while (<condition>)
{
    foreach (var o in device.GetAllObjects())
    {
        Console.WriteLine($"{o.ToString()}");
    }
}
```