namespace Iot.Device.Ad7193
{
    public class AdcValue
    {
        public byte Channel { get; set; }
        public uint Raw { get; set; }
        public double Voltage { get; set; }
        public long Time { get; set; }

        public override string ToString()
        {
            return $"Ch{Channel.ToString().PadLeft(5, ' ')} {Voltage.ToString("0.0000").PadLeft(10, ' ')} V";
        }
    }
}
