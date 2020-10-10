# Tlc1543

## Summary

High-speed 10-bit switched-capacitor successive-approximation A/D Converter with 14 channels (11 inputs and 3 self-test channels)

## Device Family

**[TLC1542/3]**: [Datasheet](https://www.ti.com/lit/ds/symlink/tlc1543.pdf)

## Binding Notes

Only mode implemented is Fast Mode 1 (10 clocks and !ChipSelect high between conversion cycles). 
Respective timing diagram can be seen on figure 9 in datasheet.

It is possible to change ADC charge channel.

```c#
_adc.ChargeChannel = Channel.SelfTest0;
```

Using EndOfConversion mode is not yet supported.

### Available methods to use are:

#### int ReadChannel(Channel)

Simple way to poll one channel.
Uses 2 cycles:

- In first cycle sends address to read from

- In second one sends dummy ChargeChannel address and reads sent value

#### List<int> ReadChannels(List<Channel>)

Fastest way to read channels from the list **in order in the list**

It is possible to aquire data from sensor out of normal order by creating list like this:

```c#
private List<Channel> channelList = new List<Channel>
{
    Channel.A4,
    Channel.A2,
    Channel.A0,
    Channel.A1,
    Channel.A3
};
List<int> values = adc.ReadChannels(_channelList);
```

Uses only one more cycle than number of channels being polled (last cycle sends dummy ChargeChannel address and reads data)

#### *int ReadChannel(int)

1 cycle long mode. To get usable data you need to use this method atleast 2 times. Remember that output that you get from this from last cycle. Use it at your own risk.
