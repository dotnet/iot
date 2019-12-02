# Pigpio

This GpioDriver implementation allows you to remotely control a Raspberry Pi's GPIO pins via a wired or wireless network connection. It is compatible with (at time of writing) every Raspberry Pi including Model A, A+, B, B+, Zero, ZeroW, Pi2B, Pi3B (Pi4 support is currently experimental).

## Setting up the Raspberry Pi

1. Install a recent version of Raspbian onto the Pi; Stretch or Buster is fine.
2. Make sure the Pi has network access so either via a wired or wireless connection
3. Configured the raspberry pi to allow remote GPIO control by following the steps in section 4.1 [here](https://gpiozero.readthedocs.io/en/stable/remote_gpio.html#preparing-the-raspberry-pi).

And you're done.

## Running the sample

The sample application (Pigpio.Sample) will periodically set GPIO4 high then low. If you connected an LED (with a current limiting resistor) to GPIO4 you then run the application you should see if turn on or off every second.

To run the sample you'll need to determine the IP address of the Pi you configured above and specify it as the first argument to the application; for example:

```
Pigpio.Sample.exe 192.168.1.101
```

If all goes to plan, you should see something like [this](https://www.youtube.com/watch?v=F9m0fqZjOGQ)

