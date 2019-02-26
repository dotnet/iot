using System;

namespace Iot.Device.Mfrc522
{
    public class Mfrc522Exception : Exception
    {
        public Mfrc522Exception(string message) 
            : base(message)
        {

        }
    }
}
