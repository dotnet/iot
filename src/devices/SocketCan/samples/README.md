# SocketCan

APIs are under `Iot.Device.SocketCan` namespace.

## Reading a frame

```csharp
using (CanRaw can = new CanRaw())
{
    CanFrame frame = new CanFrame();
    while (true)
    {
        can.ReadFrame(ref frame);
        if (frame.IsValid)
        {
            string type = frame.ExtendedFrameFormat ? "EFF" : "SFF";
            string dataAsHex = string.Join("", frame.Data.ToArray().Select((x) => x.ToString("X2")));
            Console.WriteLine($"Id: {frame.Id} [{type}]: {dataAsHex}");
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
using (CanRaw can = new CanRaw())
{
    byte[] buffer = new byte[8] { 1, 2, 3, 40, 50, 60, 70, 80 };
    CanFrame frame = new CanFrame();
    frame.StandardId = Id;

    while (true)
    {
        frame.Data = buffer;
        can.WriteFrame(ref frame);
        string dataAsHex = string.Join("", frame.Data.ToArray().Select((x) => x.ToString("X2")));
        Console.WriteLine($"Sending: {dataAsHex}");
        Thread.Sleep(1000);
    }
}
```

## Filtering

```csharp
using (CanRaw can = new CanRaw())
{
    // first argument is true if id (0x1A) is extended
    can.Filter(false, 0x1A);
    CanFrame frame = new CanFrame();

    while (true)
    {
        can.ReadFrame(ref frame);
        if (frame.IsValid)
        {
            string type = frame.ExtendedFrameFormat ? "EFF" : "SFF";
            string dataAsHex = string.Join("", frame.Data.ToArray().Select((x) => x.ToString("X2")));
            Console.WriteLine($"Id: {frame.Id} [{type}]: {dataAsHex}");
        }
        else
        {
            Console.WriteLine($"Invalid frame received!");
        }
    }
}
```
