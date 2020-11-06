# Conventions for devices APIs

This document is meant to provide guidelines. If you think the guidelines don't make sense in a specific case (or at all) please file an issue or mention it in the PR description.

## Expose everything device has vs. expose simple functionality

All APIs should be as simple as they can be and simplicity is the most important when designing APIs.

It is acceptable for the device to provide full functionality exposed by the device but if full functionality makes simple stuff hardly discoverable or harder to reason about then you should consider making advanced functionality `protected` or move it to a separate class.

I.e. every thermometer should have a way of getting a temperature (i.e. `GetTemperature`). If your thermometer is special (i.e. very high resolution) exposing internal representation might make sense but for regular thermometer it likely will not. On the other hand exposing way of calibrating the thermometer is recommended (but not required). Going further if your thermometer exposes 100 calibration parameters it probably would make it hard to see what the device actually was meant for and therefore such parameters should be made `protected` (so that derived class can still access it) or moved to a separate class - it is fine to expose one calibration method directly and move remainder elsewhere.

There is no specific guidelines what simple and main scenario is but you should consider what is the most likely thing user will do with your device and you should think if it will be useful and if it is exposed as simple as possible. If reality will be different than initial assumptions the device can be modified later.

## Value returned by sensor

- Methods (i.e. `ReadTemperature`/`GetTemperature`) should be used for anything which may change between the calls
- Properties (i.e. `SomeRegister`) should be used when value does not change between the calls (except when changed by setter)
- Use `double` when you need to return any floating point value
- Async methods can be added but the name of such method should have an `Async` suffix and there should also be synchronous equivalent method
- When method may or not return the (valid) value use `bool TryGetTemperature(out Temperature temperature)` pattern (`TryRead` and any `TryVerb` is acceptable as well) rather than returning `double.NaN` or some other sentinel value
  - exception to this are values which are not meant to be further processed and are not ambiguous (i.e. lack of pin number can be represented as `-1` because `-1` is an invalid pin number and pin numbers are not meant to be processed)
- Use `Vector2/3/4` for returning vectors`*`
- Value should conform to units conventions (see below)
- Only the most useful APIs should be public, anything else which may be useful but will unlikely get used by most of the people should be protected (inheriting the class allows you to use it but it is not visible by default) or internal/private
- Constructor should only require parameters it cannot work without - anything else should be a default value
  - For more advanced settings or devices which require many parameters using designated Settings class is recommended
- Integer values (i.e. pin number) should use `-1` as invalid/unassigned value (as opposed to `null` and `Nullable<int>`)
- If your device has internal register create an enum for the addresses (i.e. `enum Register : byte`)

`*` -  Vector3 is currently backed by float, this may be changed in the future: https://github.com/dotnet/corefx/issues/25334

## Units

Use [UnitsNet](https://github.com/angularsen/UnitsNet) whenever it is possible on any public functions, event or properties. This supports a lot of different types of units. 

If your sensor/binding unit is not present in UnitsNet, then most common case units should match [International System of Units (aka. SI)](https://en.wikipedia.org/wiki/International_System_of_Units).

Find examples of [Do and Don't here](../src/devices/README.md#using_units).

## Lifetime management (or what should be disposed by what)

Multiple devices take GpioController, PwmChannel, I2cDevice or SpiDevice as an input.
Frequently emerging question is if implemented device should dispose of such input object or not.

General rule is: If object cannot be concurrently used by multiple devices then it should be disposed.

With **I2cDevice, SpiDevice and PwmChannel** this is rather clear - object is related with a particular device and cannot be concurrently with anything. There is 1:1 correlation that one object is related with one piece of hardware and cannot be re-used without re-plugging the device and therefore **should be disposed by the device**.

For **GpioController** situation is less clear because one GpioController can be used by multiple devices: different pins on the same controller can be related to different devices. This situation is possible but in most common case new instance of GpioController should be created per device and it should be used to group pins and therefore it **should be disposed by the device**. Having said that GpioController even if designed to be one per devices is possible to use by multiple of them and **it is acceptable to add an optional constructor flag (i.e. `shouldDispose`) defaulting to disposing**. Find an example [here of a good implementation](../src/devices/README.md#gpiocontroller)

Example of such justified case could be imaginary `Led` class which wraps one pin. When new imaginary class `LedController` would show up which would wrap multiple `Led`s the input GpioController cannot be safely passed to `Led` class as it would be disposed when `Led` is no longer needed and therefore `Led` class would need to have an option of not disposing it.

## Notes

If you believe for your specific device it makes sense to break any of the rules it might be acceptable - please make sure to mention that in the pull request description.

Find examples of [Do and Don't here](../src/devices/README.md).
