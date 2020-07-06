# STUSB4500 - Autonomous USB-C PD controller for Power Sinks / UFP

## Summary
By default a USB Type-C port on your hardware can get only 5V from a USB-PD Power Source (Host / DFP)
This product enables to automatically negociate with the Source a higher Voltage (>5V) up to 100W (20V@5A).
For instance, if the Power brick can provide 4 power profiles (5V, 9V, 15V and 20V), then the STUSB4500 will request the highest voltage available (20V).
Another example, if the Power brick can provide 4 power profiles (5V, 9V, 15V and 20V) but the Application needs 9V to boot, then the STUSB4500 can be programmed to always request 9V.
This part can be easily implemented in a battery charger with USB-C input in the application.

The device doesn't need any software to run (it is automous). But it is possible to connect to this device by I2C to take control over the default device behavior, or to get the power information (Voltage/Current) of the attached power source at the other side of the cable. 

## Usage

```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, StUsb4500.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using(StUsb4500 stUsb = new StUsb4500(device))
{
    CableConnection connection = stUsb.CableConnection;
    
    if (connection != CableConnection.Disconnected)
    {
        double voltage = stUsb.RequestedVoltage;
        RequestDataObject rdo = stUsb.RequestDataObject;
        double availablePower = voltage * rdo.MaximalCurrent;
    }
}
```

## References 
https://github.com/usb-c/STUSB4500
