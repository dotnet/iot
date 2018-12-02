# Sample Application: DeviceApiTester

This sample application is primarily for testing the GPIO drivers and surrounding infrastructure.
After publishing to a target device, simply run `DeviceApiTester` to see the command line syntax.

One notable difference between the samples and real-world usage is that, because this sample
allows for changing the GPIO driver from the command line, the base `GpioDriverCommand` class
has the `CreateGpioController` method. In practice, though, you would simply create the
`GpioController` directly:

```CSharp
using (var gpio = new GpioController())
{
    ...
}
```

Similarly, the same note applies to the SPI, I2C, and PWM test samples.

To simplify the command line parsing and adding new samples, this application uses the
[CommandLineParser](https://github.com/commandlineparser/commandline) package. This simple parsing
framework automatically creates help text based on attributes of the classes in the `Commands`
folder, such as the `BlinkLedCommand` class.

Each of the commands has an `ExecuteAsync` or `Execute` method at the top of the class, which is where the
sample usage is actually demonstrated. The automatic properties of the class represent options for
the command, which are set by the command line parsing framework. The attributes on the command and
its properties are used to setup the command line.

## Example Usage

1. See a list of all commands:<br/>
`DeviceApiTester help`

2. Show the `gpio-button-event` command's syntax:<br/>
`DeviceApiTester help gpio-button-event`

3. Run the command with required parameters:<br/>
`DeviceApiTester gpio-button-event --button-pin 5 --led-pin 6`

4. Run the command with many optional parameters:<br/>
`DeviceApiTester gpio-button-event --scheme Board --button-pin 29 --pressed-value Falling --led-pin 31 --on-value Low --driver RPi3`

5. Run the command with many optional parameters using single-characters options:<br/>
`DeviceApiTester gpio-button-event -s Board -b 29 -p Falling -l 31 --on-value Low -d RPi3`

## Adding additional test samples

1. Derive a new class from one of the protocol base command classes, e.g., `GpioCommand`, `I2cCommand`, `SpiCommand`.
Deriving from one of these classes automatically gives your command the switches necessary to setup that type of connection.
    ```CSharp
    namespace DeviceApiTester.Commands.I2c
    {
        public class MySample : I2cCommand
        {
        }
    }
    ```
    - It's easiest to begin by copying one of the existing samples.
    - Please keep the new command class within the folder for connection type:<br/>
      `Commands/Gpio`, `Commands/I2c`, `Commands/Spi`.

2. Add or modify the `Verb` attribute to describe the command:
    ```CSharp
        [Verb("i2c-my-sample", HelpText = "This sample does something with an I2C connection.")]
    ```
    - Please prefix the command's verb with its connection type: `gpio-`, `i2c-`, `spi-`

3. Implement either the `ICommandVerb` or `ICommandVerbAsync` interface on your command class:
    ```CSharp
        public class MySample : I2cCommand, ICommandVerb
        {
            public int Execute()
            {
                Console.WriteLine($"BusId={BusId}, DeviceAddress={DeviceAddress}, Device={Device}");
                return 0;
            }
        }
    ```
    or
    ```CSharp
        public class MySample : I2cCommand, ICommandVerbAsync
        {
            public async Task<int> ExecuteAsync()
            {
                Console.WriteLine($"BusId={BusId}, DeviceAddress={DeviceAddress}, Device={Device}");
                await Task.Delay(500);
                return 0;
            }
        }
    ```

4. Add automatic properties for command options:
    ```CSharp
        [Option('c', "cool-option", HelpText = "A cool option argument", Required = false, Default = 0)]
        public int CoolOption { get; set; }
    ```

5. Use the properties in your execution:
    ```CSharp
        public int Execute()
        {
            Console.WriteLine($"CoolOption={CoolOption}, BusId={BusId}, DeviceAddress={DeviceAddress}, Device={Device}");
    ```

6. Deploy and run:
    ```
        DeviceApiTester help
        DeviceApiTester help i2c-my-sample
        DeviceApiTester i2c-my-sample --cool-option 42
    ```
