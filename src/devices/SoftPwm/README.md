# Software PWM

Software PWM is necessary when you cannot use hardware PWM. PWM is used for example to control and pilot servo motors.

## How to Uue

SoftPwm is part of ```System.Device.Pwm.Drivers``` and is a driver for the ```System.Device.Pwm.PwmController``` class. You can then create a software PWM like this:

```csharp
var PwmController = new PwmController(new SoftPwm());
```

By default SoftPwm is using a low priority clock to emulate the PWM. It is ok if you have leds and other non time sensitive elements attached.

**Important:** if you have clock sensitive elements attached to this software PWM, you need to use a high precision timer. In order to make it happen, you need to select it at creation time by passing ```true``` in the constructor:

```csharp
var PwmController = new PwmController(new SoftPwm(true));
```

## Key elements

Compare to hardware PWM, software PWM only needs a GPIO Pin and the channel parameter is ignored. So you can pass any ```int``` value, the channel, will always been ignore.

The first parameter has to be the GPIO and will always be checked and has to be the same.

### Opening the PWM

Usage is the same as the PwmController as the SoftPwm fully implement the needed classes.

This example shows how to open the GPIO 17 as a software PWM. The channel, here 0, is always ignored. It is used for hardware PWM.

```csharp
PwmController.OpenChannel(17, 0);
```

### Starting the PWM

Usage is the same as the PwmController. You first need to open the Channel before starting the PWM.

This example shows how to start the PWM, previously open on GPIO 17. It will start a 50Hz software PWM with 50% duty cycle:

```csharp
PwmController.StartWriting(17,0, 50, 50);
```

### Change Duty Cycle

Usage is the same as the PwmController. You first need to open the Channel before starting the PWM.

This example shows how to change the duty cycle of the PWM, previously open on GPIO 17. Duty cycle is changed for 25% with immediate effect if the PWM is already started.

```csharp
PwmController.ChangeDutyCycle(17, 0, 25);
```

### Stop the PWM

Usage is the same as the PwmController. You first need to open the Channel before starting the PWM.

This example will stop the PWM previously open on GPIO 17.

```csharp
PwmController.StopWriting(17, 0);
```

### Close the PWM

Usage is the same as the PwmController. You first need to open the Channel before starting the PWM.

This example will release the GPIO 17 previously open. The GPIO is available for other usage.

```csharp
PwmController.CloseChannel(17, 0);
```

Closing the PWM will as well release the thread used for the PWM clock.

## Performance Considerations

The high precision software PWM is resource intensive and is using a high priority thread. You may have resources issues if you are using multiple high precision software PWM. Always prefer hardware PWM to software PWM when you can have access. 


