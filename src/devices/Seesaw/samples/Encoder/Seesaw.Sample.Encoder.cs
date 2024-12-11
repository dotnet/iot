// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Seesaw;

const byte AdafruitSeesawRotaryEncoderI2cAddress = 0x36;
const byte AdafruitSeesawRotaryEncoderI2cBus = 0x1;
const byte AdafruitSeesawRotaryEncoderPushSwitchPin = 24;

const int EncoderPositionInitialValue = 100;
const int HostInterruptPin = 6;

using GpioController gpioController = new();
using Seesaw seesawDevice = new(I2cDevice.Create(new I2cConnectionSettings(AdafruitSeesawRotaryEncoderI2cBus, AdafruitSeesawRotaryEncoderI2cAddress)));

// set initial encoder position value
seesawDevice.SetEncoderPosition(EncoderPositionInitialValue);

// enable interrupt for position changes on Seesaw encoder
seesawDevice.EnableEncoderInterrupt();

// enable interrupt for rotary encoder push switch
uint encoderPushSwitchPinMask = 1U << (AdafruitSeesawRotaryEncoderPushSwitchPin);
seesawDevice.SetGpioPinMode(AdafruitSeesawRotaryEncoderPushSwitchPin, PinMode.InputPullUp);
seesawDevice.SetGpioInterrupts(encoderPushSwitchPinMask, true);

// enable host interrupt and register callback
gpioController.OpenPin(HostInterruptPin, PinMode.InputPullUp);
gpioController.RegisterCallbackForPinValueChangedEvent(HostInterruptPin, PinEventTypes.Falling, (s, e) =>
{
    uint interruptFlags = seesawDevice.ReadGpioInterruptFlags();
    if ((interruptFlags & encoderPushSwitchPinMask) > 0)
    {
        // interrupt for push switch pin -> push switch status changed
        bool pushSwitchReleased = seesawDevice.ReadGpioDigital(AdafruitSeesawRotaryEncoderPushSwitchPin);
        Console.WriteLine($"Encoder switch {(pushSwitchReleased ? "released" : "pressed")}");
        return;
    }

    int encoderPosition = seesawDevice.GetEncoderPosition();
    Console.WriteLine($"Encoder position changed: {encoderPosition}");
});

// press Enter to exit application
Console.ReadLine();
