# BrickPi3

The provided examples covers almost all high level sensor classes as well as motors and brick information. This is the code used to test the hardware.

# Schematic

Just plug your BrickPi3 shield on your Raspberry Pi. You can add multiple BrickPi3, up to 254. Please note that you'll have to setup individually each of them first by changing their address. 

If you are using another board with compatible pins with the Raspberry Pi, you will be able to use it as well without any change. If you are using a board with a different pin out, make sure you connect the SPI of your board to the SPI of BrickPi3.

![image how it works](../BrickPi3-How-it-Works.jpg)

More inrofmation on [Dexter Industries website](https://www.dexterindustries.com/BrickPi/brickpi3-technical-design-details/).

# Code

The code illustrate how to use most sensors as well as motors. It is the code used to test the hardware devices. As exampled in the main documentation, we do recommend to use the high level classes to access the sensors and the motors rather than the low level one. That said the example contains examples on how to use all of them.

Refer to the code to understand on which port you'll need to plug motors and sensors. The available tests are the following:

```
./BrickPiHardwareTest -arg1 - arg2
where -arg1, arg2, etc are one of the following:
-nobrick: don't run the basic BrickPi tests.
-motor: run basic motor tests, motors need to be on port A and D.
-vehicule: run a vehicule test, motors need to be on port A and D.
-multi: run a multi sensor test
   EV3TouchSensor on port 1
   NXTTouchSensor on port 2
   NXTColorSensor on port 3
   NXTSoundSensor on port 4
   Press the EV3TouchSensor sensor to finish
-color: run an EV3 color test
   EV3TouchSensor on port 1
   EV3ColorSensor on port 2
-touch: run touch sensor test
   EV3TouchSensor on port 1
-nxtlight: run the NXT light sensor tests
   NXTLightSensor on port 4
-nxtus: run NXT Ultrasonic test on port 4
-nxtcolor: run NXT Color sensor test
   EV3TouchSensor on port 1
   NXTColorSensor on port 4
-irsensor: run EV3 IR sensor test on port 4
```

You always have to create a brick and initialize it. Then you can run your code. In this example, reading a Touch sensor. And at the end, reset the brick.

```csharp
Brick _brick = new Brick();
_brick.InitSPI();
Console.WriteLine("Running 100 reads on EV3 touch sensor on port 1.");
EV3TouchSensor touch = new EV3TouchSensor(_brick, BrickPortSensor.PortS1);
// Alternative to test NXT touch sensor
// NXTTouchSensor touch = new NXTTouchSensor(brick, BrickPortSensor.PORT_S2);
int count = 0;
while (count < 100)
{
    Console.WriteLine($"NXT Touch, IsPRessed: {touch.IsPressed()}, ReadAsString: {touch.ReadAsString()}, Selected mode: {touch.SelectedMode()}");
    Task.Delay(300).Wait(); ;
}
_brick.ResetAll();
```