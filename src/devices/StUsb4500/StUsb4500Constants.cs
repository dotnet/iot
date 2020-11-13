// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Usb
{
    internal static class StUsb4500Constants
    {
        public const byte STUSBMASK_ATTACHED_STATUS = 0x01;
        public const byte VALUE_NOT_ATTACHED = 0;
        public const byte VALUE_ATTACHED = 1;

        public const byte MASK_REVERSE = 0x80;
        public const byte SOFT_RESET = 0x0D;
        public const byte SEND_COMMAND = 0x26;

        public const byte FTP_CUST_PASSWORD = 0x47;

        public const byte FTP_CUST_PWR = 0x80;
        public const byte FTP_CUST_RST_N = 0x40;
        public const byte FTP_CUST_REQ = 0x10;
        public const byte FTP_CUST_SECT = 0x07;
        public const byte FTP_CUST_SER_MASK = 0xF8;
        public const byte FTP_CUST_OPCODE_MASK = 0x07;

        public const byte READ = 0x00;
        public const byte WRITE_PL = 0x01;
        public const byte WRITE_SER = 0x02;
        public const byte ERASE_SECTOR = 0x05;
        public const byte PROG_SECTOR = 0x06;
        public const byte SOFT_PROG_SECTOR = 0x07;

        public const byte SECTOR_0 = 0x01;
        public const byte SECTOR_1 = 0x02;
        public const byte SECTOR_2 = 0x04;
        public const byte SECTOR_3 = 0x08;
        public const byte SECTOR_4 = 0x10;
    }
}
