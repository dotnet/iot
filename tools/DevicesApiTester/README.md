# DeviceApiTester

DeviceApiTester is a sample application primarily for testing the GPIO drivers and surrounding infrastructure.  After publishing the app to a target device, simply run `DeviceApiTester` to see the command line syntax.

One notable difference between the samples and real-world usage is that, because this sample allows for changing the GPIO driver from the command line, the base `GpioCommand` class has the `CreateGpioController` method. In practice, though, you would simply create the `GpioController` directly:

```csharp
using (var gpio = new GpioController())
{
    ...
}
```

Similarly, the same note applies to the SPI, I2C, and PWM test commands.

To simplify the command line parsing and adding new command, this application uses the
[CommandLineParser](https://github.com/commandlineparser/commandline) package. This simple parsing framework automatically creates help text based on attributes of the classes in the `Commands` folder, such as the `GpioBlinkLed` class.

Each of the commands has an `ExecuteAsync` or `Execute` method at the top of the class, which is where the sample usage is actually demonstrated. The automatic properties of the class represent options for the command, which are set by the command line parsing framework. The attributes on the command and its properties are used to setup the command line.

## Examples

1. See a list of all commands:

    `DeviceApiTester help`

2. Show the `gpio-button-event` command's syntax:

    `DeviceApiTester help gpio-button-event`

3. Run the command with required parameters:

    `DeviceApiTester gpio-button-event --button-pin 5 --led-pin 6`

4. Run the command with many optional parameters:

    `DeviceApiTester gpio-button-event --scheme Board --button-pin 29 --pressed-value Falling --led-pin 31 --on-value Low --driver RPi3`

5. Run the command with many optional parameters using single-characters options:

    `DeviceApiTester gpio-button-event -s Board -b 29 -p Falling -l 31 --on-value Low -d RPi3`

## Adding Additional Commands

1. Derive a new class from one of the protocol base command classes, e.g., `GpioCommand`, `I2cCommand`, `SpiCommand`.  Deriving from one of these classes automatically gives your command the switches necessary to setup that type of connection.

    ```csharp
    namespace DeviceApiTester.Commands.I2c
    {
        public class MyCommand : I2cCommand
        {
        }
    }
    ```

    - It's easiest to begin by copying one of the existing commands.
    - Keep the new command class within the folder for connection type:

      `Commands/Gpio`, `Commands/I2c`, `Commands/Spi`.

2. Add or modify the `Verb` attribute to describe the command:

    ```csharp
    [Verb("i2c-my-command", HelpText = "This command does something with an I2C connection.")]
    ```

    - Prefix the command's verb with its connection type: `gpio-`, `i2c-`, `spi-`  

3. Implement either the `ICommandVerb` or `ICommandVerbAsync` interface on your command class:

    ```csharp
    public class MyCommand : I2cCommand, ICommandVerb
    {
        public int Execute()
        {
            Console.WriteLine($"BusId={BusId}, DeviceAddress={DeviceAddress}");
            return 0;
        }
    }
    ```

    or

    ```csharp
    public class MyCommand : I2cCommand, ICommandVerbAsync
    {
        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine($"BusId={BusId}, DeviceAddress={DeviceAddress}");
            await Task.Delay(500);
            return 0;
        }
    }
    ```

4. Add automatic properties for command options:

    ```csharp
    [Option('c', "cool-option", HelpText = "A cool option argument", Required = false, Default = 0)]
    public int CoolOption { get; set; }
    ```

5. Use the properties in your execution:

    ```csharp
    public int Execute()
    {
        Console.WriteLine($"CoolOption={CoolOption}, BusId={BusId}, DeviceAddress={DeviceAddress}");
        ...
    }
    ```

6. Deploy and run:

    ```shell
    DeviceApiTester help
    DeviceApiTester help i2c-my-command
    DeviceApiTester i2c-my-command --cool-option 42
    ```
