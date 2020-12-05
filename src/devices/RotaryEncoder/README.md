# Quadrature Rotary Encoder

## Summary

A Rotary encoder is a device that detects angular position. One use of this is similar to a volume control on an FM radio where the user turns a shaft and the loudness of the broadcast is changed. Incremental rotary encoders do not provide information on their exact position but supply information about how much they have moved and in which direction.

![image of rotary encoder](pec11r.png)

Typically a quadrature rotary encoder will have two outputs A and B, perhaps called clock and data. For each part of a rotation then the A pin will provide a clock signal and the B pin will provide a data signal that is out of phase with the clock. The sign of the phase difference between the pins indicates the direction of rotation.

![encoder](encoder.png)

From above if we look at Pin B (data) at the time of a falling edge on Pin A (clk) then the if the value of pin P is 1 then the direction is clockwise and if it is 0 then the rotation is counter clockwise.

## Binding Notes

This binding implements scaled quadrature rotary encoder as `ScaledQuadratureEncoder`. The value is a double. You can for example set it up as a tuning dial for an FM radio with a range of 88.0 to 108.0 with a step of 0.1.

The code below shows an example of using the encoder as an FM tuner control.

```csharp
// create a RotaryEncoder that represents an FM Radio tuning dial with a range of 88 -> 108 MHz
ScaledQuadratureEncoder encoder = new ScaledQuadratureEncoder(pinA: 5, pinB: 6, PinEventTypes.Falling, pulsesPerRotation: 20, pulseIncrement: 0.1, rangeMin: 88.0, rangeMax: 108.0) { Value = 88 };
// 2 milliseconds debonce time
encoder.Debounce = TimeSpan.FromMilliseconds(2);
// Register to Value change events
encoder.ValueChanged += (o, e) =>
{
    Console.WriteLine($"Tuned to {e.Value}MHz");
};
```

This binding also features 

- Debounce functionality on the clock signal.
- Acceleration so that rotating the encoder moves it further the faster the rotation.
- Events when the value changes.

Also available is a `QuadratureRotaryEncoder` binding which has properties that represent the rotation of the encoder and the raw pulses.

## Limitations

This binding is suitable for manual and small rotations where it is not a big deal if one or few rotations may be lost.

This binding **is not** suitable for motor control with a very high rate and very precise number of counts.

The precision really depends of the hardware you are using and it is not possible to give specific range of usage. You may have to try to understand if this is working for you or not.
