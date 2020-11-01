# SPI, GPIO and I2C drivers for Arduino

This binding supports GPIO, PWM, SPI and I2C access from a normal Desktop environment (Windows, Linux) trough an Arduino board.

## Device family

This binding remotely controls different Arduino boards directly from PC Software. It provides support for accessing GPIO ports as well as I2C devices, SPI devices, PWM output and analog input. The Arduino is remote controlled by individual commands from the PC, the entire program will run on the PC, and not on the Arduino, so the connection cannot be removed while the device is being used. 

## Desktop Requirements

In order to have an Arduino board working with the PC, you need to Install the Arduino IDE together with the drivers for your board type. If you get a simple sketch uploaded and running (such as the blinking LED example) you are fine to start. If you're new to the Arduino world, read the introduction at https://www.arduino.cc/en/Guide for a quick start. The explanations below assume you have the Arduino board connected trough an USB cable with your PC and you know how to upload a sketch. 

## Preparing your Arduino
### Quick start
You need to upload a special sketch to the Arduino. This sketch implements the "Firmata-Protocol", a communication protocol that allows to remotely control all the inputs and outputs of the board. See https://github.com/firmata/protocol/blob/master/protocol.md for details. We call this sketch (or variants of it, see below) the Firmata firmware.

The binding requires Firmata Version 2.6, which is implemented i.e. by the ConfigurableFirmata project. 
- Open the Arduino IDE
- Go to the library manager and check that you have the "ConfigurableFirmata" library installed
- Open "ConfigurableFirmata.ino" from the device binding folder or go to http://firmatabuilder.com/ to create your own custom firmata firmware. Make sure you have at least the features checked that you will need.
- Upload this sketch to your Arduino. 

After these steps, you can start coding with Iot.Devices.Arduino and make your Arduino do whatever you want, from blinking LEDS to your personal weather station. For usage and examples see the samples folder. Note that ConfigurableFirmata uses a default UART speed of 57600 baud. It is recommended to increase it to 115200, though. 
### Advanced features
Some of the features require extended features on the Arduino firmware. These include SPI support and DHT sensor support. These features didn't make it 
into the main branch yet, therefore these additional steps are required:
- Go to C:\users\<username>\documents\arduino\libraries and delete the "ConfigurableFirmata" folder (save any work if you've changed anything there)
- Replace it with a clone of https://github.com/pgrawehr/ConfigurableFirmata and switch to branch "develop". 
- Make sure you have the "DHT Sensor Library" from Adafruit installed (use the library manager for that).
- You can now enable the DHT and SPI features at the beginning of the ConfigurableFirmata.ino file.
- Compile and re-upload the sketch. 

## Usage
See the example for advanced usage instructions. 

Basic start:
```
            // Portname is "COM3", "COM4" on Windows, "/dev/ttyUSB0" or similar on linux
            using (var port = new SerialPort(portName, 115200))
            {
                Console.WriteLine($"Connecting to Arduino on {portName}");
                try
                {
                    port.Open();
                }
                catch (UnauthorizedAccessException x)
                {
                    Console.WriteLine($"Could not open COM port: {x.Message} Possible reason: Arduino IDE connected or serial console open");
                    return;
                }

                ArduinoBoard board = new ArduinoBoard(port.BaseStream);
                try
                {
                    board.LogMessages += BoardOnLogMessages; // Get log messages
                    board.Initialize();
                    Console.WriteLine($"Connection successful. Firmware version: {board.FirmwareVersion}, Builder: {board.FirmwareName}");
                    // Add code that uses the board here.
                }
                catch (TimeoutException x)
                {
                    Console.WriteLine($"No answer from board: {x.Message} ");
                }
                finally
                {
                    port.Close();
                    board?.Dispose();
                }
            }
 ```
 
On Windows, only one application can use the serial port at a time, therefore you'll get "permission denied" errors when you try to run your program while i.e. the Serial Port Monitor of the Arduino IDE is open when you start your program. On the other hand, trying to upload a new sketch while the program runs will also fail.

## Known limitations

All communication is routed trough the USB cable immitating a serial port with a limited bandwith. Therefore, some not insignificant delays are to be expected when sending commands or retrieving data. Communicating with sensors which have time-critical behavior will most likely not work reliably for this reason and the standard bindings provided for these won't work. This includes sensors like the DHT11, DHT22 or HCSR-04. For some of these, special Firmata modules are available to execute the time-critical part directly on the Arduino. This problem does not exist for sensors using I2C or SPI protocols. 

For the moment this binding supports GPIO, Analog In, SPI, I2C and DHT on all platforms. Here is the list of tested features:

- [x] SPI master support for Windows 64/32
- [x] I2C master support for Windows 64/32
- [x] Basic GPIO support for Windows 64/32
- [x] Advanced GPIO support for Windows 64/32
- [x] Analog input support on Windows 64/32
- [ ] SPI support for MacOS 
- [ ] I2C support for MacOS
- [ ] GPIO support for MacOS
- [ ] SPI support for Linux 64
- [ ] I2C support for Linux 64
- [ ] GPIO support for Linux 64
