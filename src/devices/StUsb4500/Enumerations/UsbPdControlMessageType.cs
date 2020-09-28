// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Usb.Enumerations
{
    /// <summary>
    /// USB PD message types.
    /// </summary>
    internal enum UsbPdControlMessageType : byte
    {
        USBPD_CTRLMSG_Reserved1 = 0x00,
        USBPD_CTRLMSG_GoodCRC = 0x01,
        USBPD_CTRLMSG_GotoMin = 0x02,
        USBPD_CTRLMSG_Accept = 0x03,
        USBPD_CTRLMSG_Reject = 0x04,
        USBPD_CTRLMSG_Ping = 0x05,
        USBPD_CTRLMSG_PS_RDY = 0x06,
        USBPD_CTRLMSG_Get_Source_Cap = 0x07,
        USBPD_CTRLMSG_Get_Sink_Cap = 0x08,
        USBPD_CTRLMSG_DR_Swap = 0x09,
        USBPD_CTRLMSG_PR_Swap = 0x0A,
        USBPD_CTRLMSG_VCONN_Swap = 0x0B,
        USBPD_CTRLMSG_Wait = 0x0C,
        USBPD_CTRLMSG_Soft_Reset = 0x0D,
        USBPD_CTRLMSG_Reserved2 = 0x0E,
        USBPD_CTRLMSG_Reserved3 = 0x0F,
        USBPD_CTRLMSG_Not_Supported = 0x10,
        USBPD_CTRLMSG_Get_Source_Cap_Extended = 0x11,
        USBPD_CTRLMSG_Get_Status = 0x12,
        USBPD_CTRLMSG_FR_Swap = 0x13,
        USBPD_CTRLMSG_Get_PPS_Status = 0x14,
        USBPD_CTRLMSG_Get_Country_Codes = 0x15,
        USBPD_CTRLMSG_Reserved4 = 0x16,
        USBPD_CTRLMSG_Reserved5 = 0x1F,
    }
}
