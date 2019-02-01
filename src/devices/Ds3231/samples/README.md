# Example of DS3231

## Hardware Required
* DS3231
* Male/Female Jumper Wires

## Circuit
![](DS3231_circuit_bb.png)

Ds3231
* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND

## Code
* First, you need to create a Ds3231 object. After that you should call Initialize() to initialize.
    ```C#
    // the program runs in Linux, initialize RTC
    Ds3231 rtc = new Ds3231(OSPlatform.Linux);
    rtc.Initialize();
    ```

* Second, set Ds3231 time
    ```C#
    rtc.SetTime(DateTime.Now);
    ```

* In the loop, read the sensor data.
    ```C#
    // loop
    while (true)
    {
        // read temperature
        double temp = rtc.ReadTemperature();
        // read time
        DateTime dt = rtc.ReadTime();

        Console.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
        Console.WriteLine($"Temperature: {temp} ℃");
        Console.WriteLine();

        // wait for a second
        Thread.Sleep(1000);
    }
    ```

## Result
![](res.jpg)
