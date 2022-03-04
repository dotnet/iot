# Virtual GPIO Controller

As the name says, `VirtualGpio` is a virtualized GPIO controller. It based on `GpioController` and defines an input method.

It allows you to control input pins of GPIO using software code without having to rely on specific hardware. For simulation, testing, etc.

# Usage

## Create vGPIO controller

```csharp
VirtualGpioController vGpio = VirtualGpioController.Create(16); // with 16 pins
```

## Add event handlers

```csharp
PinChangeEventHandler pinValueChanged = (sender, args) =>
{
    Console.WriteLine($"PinNumber: {args.PinNumber}, ChangeType: {args.ChangeType}");
};

vGpio.OutputPinValueChanged += pinValueChanged;
vGpio.InputPinValueChanged += pinValueChanged;
```

## Write output pins

```csharp
vGpio.OpenPin(0, PinMode.Output);
vGpio.Write(0, PinValue.High); // set to high -> trigger `OutputPinValueChanged` event
```

## Control input pins

```csharp
vGpio.Input(0, PinValue.High); // connect to high -> same value, nothing happend
vGpio.Input(0, PinValue.Low); // connect to low -> shorted, throw exception
```

```csharp
vGpio.OpenPin(1, PinMode.Input);
vGpio.Input(1, PinValue.High);  // connect to high -> trigger `InputPinValueChanged` event
vGpio.Input(1, null); // input Hi-Z(disconnect) -> undefined behavior -> stay high -> no event
```

```csharp
vGpio.OpenPin(2, PinMode.InputPullUp); // default high
vGpio.Input(2, PinValue.High); // connect to high -> no event
vGpio.Input(2, PinValue.Low); // connect to low -> trigger `InputPinValueChanged` event
vGpio.Input(2, null); // input Hi-Z(disconnect) -> pull-up to high -> trigger `InputPinValueChanged` event
```
