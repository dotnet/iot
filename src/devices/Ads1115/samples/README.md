# Example of ADS1115

## Hardware Required
* ADS1115
* Rotary Potentiometer
* Male/Female Jumper Wires

## Circuit
![](ADS1115_circuit_bb.png)

ADS1115
* ADDR - GND
* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND
* A0 - Rotary Potentiometer Pin 2

Rotary Potentiometer
* Pin 1 - 5V
* Pin 2 - ADS1115 Pin A0
* Pin 3 - GND

## Code
* First, you need to create a ADS1115 object. After that you should call Initialize() to initialize.
    ```C#
    // the program runs in Linux
    // set I2C bus ID: 1
    // ADS1115 Addr Pin connect to GND
    // measure the voltage AIN0
    // set the maximum range to 6.144V
    Ads1115 adc = new Ads1115(OSPlatform.Linux, 1, AddressSetting.GND, InputMultiplexeConfig.AIN0, PgaConfig.FS6144);
    adc.Initialize();
    ```

* In the loop, read the sensor data.
    ```C#
    // loop
    while (true)
    {
        // read raw data form the sensor
        short raw = adc.ReadRaw();
        // raw data convert to voltage
        double voltage = adc.RawToVoltage(raw);

        Console.WriteLine($"ADS1115 Raw Data: {raw}");
        Console.WriteLine($"Voltage: {voltage}");
        Console.WriteLine();

        // wait for 2s
        Thread.Sleep(2000);
    }
    ```

## Result
![](res.jpg)
