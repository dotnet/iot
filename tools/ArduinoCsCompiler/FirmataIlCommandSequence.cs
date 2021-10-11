using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    internal class FirmataIlCommandSequence : FirmataCommandSequence
    {
        public FirmataIlCommandSequence(ExecutorCommand ilCommand)
        : base()
        {
            Command = ilCommand;
            WriteByte((byte)CompilerCommandHandler.SchedulerData);
            WriteByte((byte)0x7F); // IL data
            WriteByte((byte)ilCommand);
        }

        public ExecutorCommand Command
        {
            get;
        }

        public static byte[] Decode7BitBytes(byte[] data, int length)
        {
            byte[] retBytes = new byte[length];

            for (int i = 0; i < length / 2; i++)
            {
                retBytes[i] = data[i * 2];
                retBytes[i] += (byte)((data[(i * 2) + 1]) << 7);
            }

            return retBytes;
        }

        /// <summary>
        /// Send a short as 2 bytes.
        /// Note: Only sends 14 bit!
        /// </summary>
        /// <param name="value">An integer value to send</param>
        public void SendUInt14(UInt16 value)
        {
            WriteByte((byte)(value & 0x7F));
            WriteByte((byte)((value >> 7) & 0x7F));
        }

        public void SendInt14(int value)
        {
            WriteByte((byte)(value & 0x7F));
            WriteByte((byte)((value >> 7) & 0x7F));
        }
    }
}
