using Iot.Units;

namespace Dhtxx.Devices
{
    public class Dht11Device : IDhtDevice
    {
        public double GetHumidity(byte[] readBuff)
        {
            return readBuff[0] + readBuff[1] * 0.1;
        }

        public Temperature GetTemperature(byte[] readBuff)
        {
            var temp = readBuff[2] + readBuff[3] * 0.1;
            return Temperature.FromCelsius(temp);
        }
    }
}
