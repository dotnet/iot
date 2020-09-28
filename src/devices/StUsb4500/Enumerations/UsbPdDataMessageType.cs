// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Usb.Enumerations
{
    /// <summary>
    /// USB PD message types.
    /// </summary>
    internal enum UsbPdDataMessageType : byte
    {
        USBPD_DATAMSG_Reserved1 = 0x00,
        USBPD_DATAMSG_Source_Capabilities = 0x01,
        USBPD_DATAMSG_Request = 0x02,
        USBPD_DATAMSG_BIST = 0x03,
        USBPD_DATAMSG_Sink_Capabilities = 0x04,
        USBPD_DATAMSG_Battery_Status = 0x05,
        USBPD_DATAMSG_Alert = 0x06,
        USBPD_DATAMSG_Get_Country_Info = 0x07,
        USBPD_DATAMSG_Reserved2 = 0x08,
        USBPD_DATAMSG_Reserved3 = 0x0E,
        USBPD_DATAMSG_Vendor_Defined = 0x0F,
        USBPD_DATAMSG_Reserved4 = 0x10,
        USBPD_DATAMSG_Reserved5 = 0x1F,
    }
}
