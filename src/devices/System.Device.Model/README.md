# System.Device.Model - attributes for device bindings

**This library is experimental, it may change and it may be moved to a different package in the future. Avoid taking direct dependency on it.**

This library provides a set of attributes which allow to annotate devices.

They can be used for:

- implementation of Azure Plug & Play
- providing extra metadata about sensors (we can i.e. generate some parts of the README file or add extra checks)

Model is representing language independent description of the device. The attributes represent a C# mapping from C# types into the model.

## InterfaceAttribute

Every class producing telemetry or exposing some commands should put a telemetry attribute on it

```csharp
    [Interface("LPS25H - Piezoresistive pressure and thermometer sensor")]
    public class Lps25h : IDisposable
```

- if class derives from class annotated with `[Interface(...)]` attribute:
  - class will inherit all annotations from the base class(es)
  - if class provides extra telemetry/command/properties it should add another InterfaceAttribute on itself
  - if class doesn't provide any extra annotations it should not have extra interface
- display name must be provided to the InterfaceAttribute

## TelemetryAttribute

Every method or property producing telemetry should have `[Telemetry]` attribute on it.
For properties providing name of the `Telemetry` is optional as it can be deduced from the property name.

Telemetry can be put on:

- properties
- methods returning value but not taking any arguments
- methods returning bool and taking one `out` argument
  - multiple `out` arguments are currently out of scope but are considered

```csharp
        [Telemetry]
        public Temperature Temperature => Temperature.FromDegreesCelsius(42.5f + ReadInt16(Register.Temperature) / 480f);

        [Telemetry("Humidity")]
        public bool TryReadHumidity(out RelativeHumidity humidity) => TryReadHumidityCore(out humidity);

        [Telemetry("Pressure")]
        public Pressure ReadPressure() { /*...*/ }
```

- if telemetry is not producing typed unit (i.e. `Vector3`) it should have additional `displayName` provided
- optional arguments are treated as if they were not there
- it's not allowed to have more than one `Telemetry` attribute with the same name on the same `Interface`

## PropertyAttribute

Properties should be put on properties or methods which describe the device or change its functioning.
They should only be used on things which don't change value between calls (unless it's been written to or a command has been executed on the device).
Specifially reading (telemetry) from the device should not change the state of the property.

Usage is similar to Telemetry with some additions:

- they can be writable
  - if same name (i.e. `PowerMode`) is used on i.e. `SetPowerMode` and `ReadPowerMode` they will be merged into a single model property
- they can be put on methods without return value taking one argument (it must not be be passed by reference)
- it's not allowed for more than one writers or readers with the same name to be present on the same interface

```csharp
[Property]
public Sampling HumiditySampling { get { /*...*/ } set { /* ... */ } }

[Property("PowerMode")]
public void SetPowerMode(Bme680PowerMode powerMode) { /*...*/ }
[Property("PowerMode")]
public Bme680PowerMode ReadPowerMode() { /*...*/ }
```

## ComponentAttribute

Components represent references to other (instances) of interfaces.
They can only be put on the properties, the return type of the property or its ancestor class should have an `Interface` attribute.

```csharp
[Component]
public SenseHatTemperatureAndHumidity TemperatureAndHumidity { get; private set; }

// ...
public class SenseHatTemperatureAndHumidity : Hts221.Hts221 { /* ... */ }
// ...
[Interface("HTS221 - Capacitive digital sensor for relative humidity and temperature")]
public class Hts221 : IDisposable { /* ... */ }
```

## CommandAttribute

Commands can be something which can be executed on the device and they can be put only on methods.

```csharp
[Command]
public void PlayTone(double frequency, int duraton) { /* ... */ }
[Command]
protected override void SetDefaultConfiguration() { /* ... */ }
```

## Type serialization

Only simple types can be serialized:

- enums (without Flags attribute)
  - values out of enum range are not permitted (i.e. bitwise combination)
- UnitsNet units
- basic C# types
- System.Numerics.Vector2, System.Numerics.Vector3, System.Numerics.Vector4
- System.Drawing.Color
