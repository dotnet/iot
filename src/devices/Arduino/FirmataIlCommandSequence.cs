using System;

namespace Iot.Device.Arduino
{
    internal class FirmataIlCommandSequence : FirmataCommandSequence
    {
        public FirmataIlCommandSequence(ExecutorCommand ilCommand)
        : base(FirmataCommand.START_SYSEX)
        {
            Command = ilCommand;
            WriteByte((byte)FirmataSysexCommand.SCHEDULER_DATA);
            WriteByte((byte)0x7F); // IL data
            WriteByte((byte)ilCommand);
        }

        public ExecutorCommand Command
        {
            get;
        }

        /// <summary>
        /// Send an int32 as 5 x 7 bits. This form of transmitting integers is only supported by extension modules
        /// </summary>
        /// <param name="value">The 32-Bit value to transmit</param>
        public void SendInt32(int value)
        {
            byte[] data = new byte[5];
            data[0] = (byte)(value & 0x7F);
            data[1] = (byte)((value >> 7) & 0x7F);
            data[2] = (byte)((value >> 14) & 0x7F);
            data[3] = (byte)((value >> 21) & 0x7F);
            data[4] = (byte)((value >> 28) & 0x7F);
            Write(data);
        }

        /// <summary>
        /// Send a short as 2 bytes.
        /// Note: Only sends 14 bit!
        /// </summary>
        /// <param name="value">An integer value to send</param>
        public void SendInt14(Int16 value)
        {
            WriteByte((byte)(value & 0x7F));
            WriteByte((byte)((value >> 7) & 0x7F));
        }
    }
}
