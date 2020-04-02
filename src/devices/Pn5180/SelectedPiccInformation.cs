using Iot.Device.Rfid;

namespace Iot.Device.Pn5180
{
    internal class SelectedPiccInformation
    {
        public Data106kbpsTypeB Card { get; set; }
        public bool LastBlockMark { get; set; }
    }
}
