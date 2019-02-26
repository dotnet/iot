# Conventions for devices APIs

## Value returned by sensor

- prefer property (i.e. Temperature) rather than method (i.e. GetTemperature)
- use `double` when you need to return any floating point value
- use `Vector2/3/4` for returning vectors`*`
- value should conform to units conventions (see below)
- only the most useful APIs should be public, anything else which may be useful but will unlikely get used by most of the people should be protected (inheriting the class allows you to use it but it is not visible by default) or internal/private
- constructor should only require parameters it cannot work without - anything else should be a default value
  - for more advanced settings or devices which require many parameters using designated Settings class is recommended
- invalid/unassigned pin should have number `-1` (as opposed to `Nullable<int>`)
- if your device has internal register create an enum for the addresses (i.e. `enum Register : byte`)

`*` -  Vector3 is currently backed by float, this may be changed in the future: https://github.com/dotnet/corefx/issues/25334

## Units

In the most common case units should match [International System of Units (aka. SI)](https://en.wikipedia.org/wiki/International_System_of_Units) with some exceptions:

- Celsius should be used for temperature

## Notes

If you believe for your specific device it makes sense to break any of the rules it might be acceptable - please make sure to mention that in the pull request description.
