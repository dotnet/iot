using System;
using System.Device.I2c;
using System.Text;

namespace DeleteMe
{
    /// <summary>
    /// No doc
    /// </summary>
    public class Program
    {
        /// <summary>
        /// No doc
        /// </summary>
        /// <param name="args">nothing</param>
        public static void Main(string[] args)
        {
            using (var bus = I2cBus.Create(1))
            {
                using (var device = bus.CreateDevice(0x50))
                {
                    device.Write(new byte[] { 0x00, 0x00 });
                    var readBuffer = new Span<byte>(new byte[32]);
                    device.Read(readBuffer);
                    var dataStr = Encoding.UTF8.GetString(readBuffer);
                    dataStr.ToString();

                    var result = device.WriteWithResult(new byte[0]);
                    result.ToString();
                }
            }
        }
    }
}
