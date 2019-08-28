# Conventions for devices APIs

## Value returned by sensor

- Methods (i.e. `ReadTemperature`/`GetTemperature`) should be used for anything which may change between the calls
- Properties (i.e. `SomeRegister`) should be used when value does not change between the calls (except when changed by setter)
- Use `double` when you need to return any floating point value
- Async methods can be added but the name of such method should have an `Async` suffix and there should also be synchronous equivalent method
- When method may or not return the (valid) value use `bool TryGetTemperature(out Temperature temperature)` pattern (`TryRead` and any `TryVerb` is acceptable as well) rather than returning `double.NaN` or some other sentinel value
  - exception to this are values which are not meant to be further processed and are not ambigious (i.e. lack of pin number can be represented as `-1` because `-1` is an invalid pin number and pin numbers are not meant to be processed)
- Use `Vector2/3/4` for returning vectors`*`
- Value should conform to units conventions (see below)
- Only the most useful APIs should be public, anything else which may be useful but will unlikely get used by most of the people should be protected (inheriting the class allows you to use it but it is not visible by default) or internal/private
- Constructor should only require parameters it cannot work without - anything else should be a default value
  - For more advanced settings or devices which require many parameters using designated Settings class is recommended
- Integer values (i.e. pin number) should use `-1` as invalid/unassigned value (as opposed to `null` and `Nullable<int>`)
- If your device has internal register create an enum for the addresses (i.e. `enum Register : byte`)

`*` -  Vector3 is currently backed by float, this may be changed in the future: https://github.com/dotnet/corefx/issues/25334

## Units

In the most common case units should match [International System of Units (aka. SI)](https://en.wikipedia.org/wiki/International_System_of_Units) with some exceptions:

- If Iot.Units defines unit it should be used - i.e. for temperature you should use Iot.Units.Temperature

## Lifetime management (or what should be disposed by what)

Multiple devices take GpioController, PwmChannel, I2cDevice or SpiDevice as an input.
Frequently emerging question is if implemented device should dispose of such input object or not.

General rule is: If object cannot be concurrently used by multiple devices then it should be disposed.

With **I2cDevice, SpiDevice and PwmChannel** this is rather clear - object is related with a particular device and cannot be concurrently with anything. There is 1:1 correlation that one object is related with one piece of hardware and cannot be re-used without re-plugging the device and therefore **should be disposed by the device**.

For **GpioController** situation is less clear because one GpioController can be used by multiple devices: different pins on the same controller can be related to different devices. This situation is possible but in most common case new instance of GpioController should be created per device and it should be used to group pins and therefore it **should be disposed by the device**. Having said that GpioController even if designed to be one per devices is possible to use by multiple of them and **it is acceptable to add an optional constructor flag (i.e. `shouldDispose`) defaulting to disposing**.

Example of such justified case could be imaginary `Led` class which wraps one pin. When new imaginary class `LedController` would show up which would wrap multiple `Led`s the input GpioController cannot be safely passed to `Led` class as it would be disposed when `Led` is no longer needed and therefore `Led` class would need to have an option of not disposing it.


## Notes

If you believe for your specific device it makes sense to break any of the rules it might be acceptable - please make sure to mention that in the pull request description.
