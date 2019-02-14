# Character LCD display Samples

Two different samples are provided. The main method will use the Board's Gpio pins to drive the LCD display. The second example will instead use an MCP Gpio extender backpack to drive the LCD display. Also the second example can use Grove RGB LCD Backlight via i2c bus. This second example has been tested on a CrowPi device and Grove LCD RGB Backlight device.

### Build configuration

Please build the samples project with configuration key depending on connected devices:

- For GPIO connection you don't have to use any configuration keys. Example:

```
dotnet publish -o C:\DeviceApiTester -r linux-arm
```

- For MCP GPIO extender backpack please use *USEI2C* key. Example:

```
dotnet publish -c USEI2C -o C:\DeviceApiTester -r linux-arm
```

- For Grove RGB LCD Backlight please use *USERGB* key. Example:

```
dotnet publish -c USERGB -o C:\DeviceApiTester -r linux-arm
```

### Sample wiring

![](lcmWiringExample.jpg)
