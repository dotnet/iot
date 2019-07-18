# SocketCan

APIs are under `Iot.Device.SocketCan` namespace.

## Reading a frame

```csharp

using (CanRaw can = new CanRaw())
{
    byte[] buffer = new byte[8];
    // to scope to specific id
    // can.Filter(id);

    while (true)
    {
        if (can.TryReadFrame(buffer, out int frameLength, out CanId id))
        {
            Span<byte> data = new Span<byte>(buffer, 0, frameLength);
            string type = id.ExtendedFrameFormat ? "EFF" : "SFF";
            string dataAsHex = string.Join("", data.ToArray().Select((x) => x.ToString("X2")));
            Console.WriteLine($"Id: 0x{id.Value:X2} [{type}]: {dataAsHex}");
        }
        else
        {
            Console.WriteLine($"Invalid frame received!");
        }
    }
}
```

## Writing a frame


```csharp
CanId id = new CanId()
{
    Standard = 0x1A // arbitrary id
};

using (CanRaw can = new CanRaw())
{
    byte[][] buffers = new byte[][]
    {
        new byte[8] { 1, 2, 3, 40, 50, 60, 70, 80 },
        new byte[7] { 1, 2, 3, 40, 50, 60, 70 },
        new byte[0] { },
        new byte[1] { 254 },
    };

    while (true)
    {
        foreach (byte[] buffer in buffers)
        {
            can.WriteFrame(buffer, id);
            string dataAsHex = string.Join("", buffer.Select((x) => x.ToString("X2")));
            Console.WriteLine($"Sending: {dataAsHex}");
            Thread.Sleep(1000);
        }
    }
}
```
