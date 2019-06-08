using Iot.Units;

namespace Dhtxx.Devices
{
    public interface IDhtDevice
    {
        double GetHumidity(byte[] readBuff);

        Temperature GetTemperature(byte[] readBuff);
    }
}
