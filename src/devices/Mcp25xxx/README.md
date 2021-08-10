# Mcp25xxx device family - CAN bus

**This binding is currently not finished. Please consider contributing to help us finish it. In the meantime consider using [SocketCan](../SocketCan/README.md)**

The MCP25XXX is a stand-alone CAN controller and includes features like faster throughput, databyte filtering, and support for time-triggered protocols.

## Documentation

- [MCP25625 PICtail Plus Daughter Board (ADM00617)](https://www.microchip.com/wwwproducts/DevTool/digikey/MCP25625)
- [MCP25625 Mini Can Bus Shield (MCP2515 compatible)](https://www.tindie.com/products/geraldjust/mcp25625-mini-can-bus-shield-mcp2515-compatible/)
- [CAN-BUS Shield](https://www.sparkfun.com/products/13262)
- [TotalPhase CAN tools](https://www.totalphase.com/protocols/can/)

MCP25XXX devices contain different markings to distinguish features like interfacing, packaging, and temperature ratings.  For example, MCP25625 contains a CAN transceiver. Please review specific datasheet for more information.

- MCP2515 [datasheet](http://ww1.microchip.com/downloads/en/devicedoc/21801e.pdf)
- MCP25625 [datasheet](http://ww1.microchip.com/downloads/en/DeviceDoc/20005282B.pdf)

## Usage

You can create a Mcp25625 device like this:

```csharp
SpiConnectionSettings spiConnectionSettings = new(0, 0);
SpiDevice spiDevice = SpiDevice.Create(spiConnectionSettings);
Mcp25625 mcp25xxx = new Mcp25625(spiDevice);
```

### Read all the registers

You can read all the registers like this:

```csharp
Console.WriteLine("Read Instruction for All Registers");
Array addresses = Enum.GetValues(typeof(Address));

foreach (Address address in addresses)
{
    byte addressData = mcp25xxx.Read(address);
    Console.WriteLine($"0x{(byte)address:X2} - {address,-10}: 0x{addressData:X2}");
}
```

to read a single register, just use the `Address` enum.

### RX Status

The RX status is available like this:

```csharp
Console.WriteLine("Rx Status Instruction");
RxStatusResponse rxStatusResponse = mcp25xxx.RxStatus();
Console.WriteLine($"Value: 0x{rxStatusResponse.ToByte():X2}");
Console.WriteLine($"Filter Match: {rxStatusResponse.FilterMatch}");
Console.WriteLine($"Message Type Received: {rxStatusResponse.MessageTypeReceived}");
Console.WriteLine($"Received Message: {rxStatusResponse.ReceivedMessage}");
```

### Read Status

The Read status is available like this:

```csharp
Console.WriteLine("Read Status Instruction");
ReadStatusResponse readStatusResponse = mcp25xxx.ReadStatus();
Console.WriteLine($"Value: 0x{readStatusResponse:X2}");
Console.WriteLine($"Rx0If: {readStatusResponse.HasFlag(ReadStatusResponse.Rx0If)}");
Console.WriteLine($"Rx1If: {readStatusResponse.HasFlag(ReadStatusResponse.Rx1If)}");
Console.WriteLine($"Tx0Req: {readStatusResponse.HasFlag(ReadStatusResponse.Tx0Req)}");
Console.WriteLine($"Tx0If: {readStatusResponse.HasFlag(ReadStatusResponse.Tx0If)}");
Console.WriteLine($"Tx0Req: {readStatusResponse.HasFlag(ReadStatusResponse.Tx0Req)}");
Console.WriteLine($"Tx1If: {readStatusResponse.HasFlag(ReadStatusResponse.Tx1If)}");
Console.WriteLine($"Tx1Req: {readStatusResponse.HasFlag(ReadStatusResponse.Tx1Req)}");
Console.WriteLine($"Tx2Req: {readStatusResponse.HasFlag(ReadStatusResponse.Tx2Req)}");
Console.WriteLine($"Tx2If: {readStatusResponse.HasFlag(ReadStatusResponse.Tx2If)}");
```

### Transmit a message

You can transmit a message like this:

```csharp
Console.WriteLine("Transmit Message");

mcp25xxx.WriteByte(
    new CanCtrl(CanCtrl.PinPrescaler.ClockDivideBy8,
        false,
        false,
        false,
        OperationMode.NormalOperation));

byte[] data = new byte[] { 0b0000_0001, 0b0010_0011, 0b0100_0101, 0b0110_0111, 0b1000_1001 };

mcp25xxx.Write(
    Address.TxB0Sidh,
    new byte[]
    {
        new TxBxSidh(0, 0b0000_1001).ToByte(), new TxBxSidl(0, 0b001, false, 0b00).ToByte(),
        new TxBxEid8(0, 0b0000_0000).ToByte(), new TxBxEid0(0, 0b0000_0000).ToByte(),
        new TxBxDlc(0, data.Length, false).ToByte()
    });

mcp25xxx.Write(Address.TxB0D0, data);

// Send with TxB0 buffer.
mcp25xxx.RequestToSend(true, false, false);
cp25xxx.RequestToSend(false, false, true);
```

**Note**: You will find detailed way of using this binding in the [sample file](samples)

## Binding Notes

More details will be added in future PR once core CAN classes/interfaces are determined.
