# Pca9685 - I2C PWM Driver sample

```csharp
using (var pca9685 = new Pca9685(device, pwmFrequency: 50))
{
    pca9685.SetDutyCycleAllChannels(0.3); // 30% fill
    PwmChannel firstChannel = pca9685.CreatePwmChannel(0); // channel 0
    PwmChannel secondChannel = pca9685.CreatePwmChannel(1); // channel 1

    firstChannel.DutyCycle = 0.0; // min
    secondChannel.DutyCycle = 1.0; // max

    // note: SetDutyCycleAllChannels cannot be used anymore
    //       because it would interfere with firstChannel and secondChannel setting
    //       this cannot be done either:
    //       pca9685.SetDutyCycle(1, 0.7);

    pca9685.SetDutyCycle(2, 0.7); // channel 2: 70%
}
```
