# Common classes

This contains a set of classes that can be used accross different sensors and bindings.

## Weather helpers

The `WeatherHelper` class contains a set of static functions to calculate meteorological data based on different input sets.

Some of the functions include:

- CalculateDewPoint: Calculates the [dew point](https://en.wikipedia.org/wiki/Dew_point), given a measured temperature and relative humidity. Simply put, the dew point indicates at what temperature water will condense if temperature drops.
- CalculateAltitude: (Different overloads) Calculate observer altitude given current pressure and temperature
- CalculateBarometricPressure: Calculate the sea-level equivalent of the current pressure, given the observers altitude. Used to make pressure readings from different locations comparable.

## Number Helper

The `NumberHelper` class contains a few methods to convert BCD numbers (binary coded decimal, uses 4 bits for each decimal digit) to binary and vice-versa.

## Logging

The `LogDispatcher` class contains static methods to enable a binding to provide log output. This can be helpful for debugging or analysis.

Logging is disabled by default. To enable logging to the console in an application, you can use this code:

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
      .AddConsole();
});

// Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
LogDispatcher.LoggerFactory = loggerFactory;
```

Other loggers are available in the `Microsoft.Extensions.Logging` Nuget-package. Some help is [here](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging#non-host-console-app). Alternatively, you can also connect the log output to your favorite logging framework, such as NLog, Log4Net and others.

To enable a binding to use logging, you can call `GetCurrentClassLogger()` in the constructor to get a logger that automatically identifies the class.

```csharp
    private ILogger _logger;

    public MyTestComponent()
    {
        _logger = this.GetCurrentClassLogger();
    }
```

Then, logging can be performed using the methods on the obtained `ILogger` interface.

```csharp
_logger.LogInformation("An informative message");
_logger.LogError("An error situation");
_logger.LogWarning(new PlatformNotSupportedException("Something is not supported"), "With exception context");
```
