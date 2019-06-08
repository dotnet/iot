using Iot.Units;

namespace Dhtxx.Devices
{
    public class Dht22Device : IDhtDevice
    {
        public double GetHumidity(byte[] readBuff)
        {
            return (readBuff[0] * 256 + readBuff[1]) * 0.1;
        }

        public Temperature GetTemperature(byte[] readBuff)
        {
            var temp = (readBuff[2] & 0x7F) + readBuff[3] * 0.1;
            // if MSB = 1 we have negative temperature
            temp = ((readBuff[2] & 0x80) == 0 ? temp : -temp);

            return Temperature.FromCelsius(temp);
        }
    }
}
