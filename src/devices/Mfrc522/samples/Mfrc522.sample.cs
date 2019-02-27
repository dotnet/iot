using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Linq;
using System.Threading.Tasks;

namespace Iot.Device.Mfrc522
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Hello Mfrc522!");

      // This sample demonstrates how to read a RFID tag
      // using the Mfrc522Controller

      var connection = new SpiConnectionSettings(0, 0);
      connection.ClockFrequency = 500000;

      var spi = new UnixSpiDevice(connection);
      using (var mfrc522Controller = new Mfrc522Controller(spi))
      {
        //await ReadCardUidLoop(mfrc522Controller);

        ReadCardAuth(mfrc522Controller);
      }

      await Task.CompletedTask;
    }

    private static void ReadCardAuth(Mfrc522Controller mfrc522Controller)
    {
      var (status, _) = mfrc522Controller.Request(RequestMode.RequestIdle);

      if (status != Status.OK)
        return;

      Console.WriteLine("Detected card");

      var (status2, cardUid) = mfrc522Controller.AntiCollision();
      Console.WriteLine($"Card UID: {string.Join(", ", cardUid)}");

      mfrc522Controller.SelectTag(cardUid);

      if (mfrc522Controller.Authenticate(RequestMode.Authenticate1A, 7, Mfrc522Controller.DefaultAuthKey, cardUid) == Status.OK)
      {
        var data = new byte[16 * 3];
        for (var x = 0; x < data.Length; x++)
        {
          data[x] = (byte)(x + 65);
        }

        for (var b = 0; b < 3; b++)
        {
          mfrc522Controller.WriteCardData((byte)(4 + b), data.Skip(b * 16).Take(16).ToArray());
        }
      }

      // Reading data
      var continueReading = true;
      for (var s = 0; s < 16 && continueReading; s++)
      {
        // Authenticate sector
        if (mfrc522Controller.Authenticate(RequestMode.Authenticate1A, (byte)((4 * s) + 3), Mfrc522Controller.DefaultAuthKey, cardUid) == Status.OK)
        {
          Console.WriteLine($"Sector {s}");
          for (var b = 0; b < 3 && continueReading; b++)
          {
            byte[] data;
            (status, data) = mfrc522Controller.ReadCardData((byte)((4 * s) + b));
            if (status != Status.OK)
            {
              continueReading = false;
              break;
            }

            Console.WriteLine($"Block {b} ({data.Length} bytes): {string.Join(" ", data.Select(x => x.ToString("X2")))}");
          }
        }
        else
        {
          Console.WriteLine("Authentication error");
          break;
        }
      }

      mfrc522Controller.ClearSelection();
    }

    private static async Task ReadCardUidLoop(Mfrc522Controller mfrc522Controller)
    {
      while (true)
      {
        var (status, _) = mfrc522Controller.Request(RequestMode.RequestIdle);

        if (status != Status.OK)
          continue;

        var (status2, uid) = mfrc522Controller.AntiCollision();
        Console.WriteLine(string.Join(", ", uid));

        await Task.Delay(500);
      }
    }
  }
}
