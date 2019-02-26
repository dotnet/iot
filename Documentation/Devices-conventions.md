# Conventions for devices APIs

## Value returned by sensor

- prefer property (i.e. Temperature) rather than method (i.e. GetTemperature)
- use `double` when you need to return any floating point value
- use `Vector2/3/4` for returning vectors`*`
- value should conform to units conventions (see below)

`*` -  Vector3 is currently backed by float, this may be changed in the future: https://github.com/dotnet/corefx/issues/25334

## Units

In the most common case units should match [International System of Units (aka. SI)](https://en.wikipedia.org/wiki/International_System_of_Units) with some exceptions:

- Celsius should be used for temperature

## Notes

If you believe for your specific device it makes sense to break any of the rules it might be acceptable - please make sure to mention that in the pull request description.
