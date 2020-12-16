// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Uri type
    /// </summary>
    public enum UriType
    {
        /// <summary>
        /// No prepending is done, and the URI field
        /// contains the unabridged URI.
        /// </summary>
        [Description("")]
        NoFormat = 0x00,

        /// <summary>
        /// http://www.
        /// </summary>
        [Description("http://www.")]
        HttpWww = 0x01,

        /// <summary>
        /// https://www.
        /// </summary>
        [Description("https://www.")]
        HttpsWww = 0x02,

        /// <summary>
        /// http://
        /// </summary>
        [Description("http://")]
        Http = 0x03,

        /// <summary>
        /// https://
        /// </summary>
        [Description("https://")]
        Https = 0x04,

        /// <summary>
        /// tel:
        /// </summary>
        [Description("tel:")]
        Tel = 0x05,

        /// <summary>
        /// mailto:
        /// </summary>
        [Description("mailto:")]
        MailTo = 0x06,

        /// <summary>
        /// ftp://anonymous:anonymous@
        /// </summary>
        [Description("ftp://anonymous:anonymous@")]
        FtpAnonymousAnonymous = 0x07,

        /// <summary>
        /// ftp://ftp.
        /// </summary>
        [Description("ftp://ftp.")]
        FtpFtp = 0x08,

        /// <summary>
        /// ftps://
        /// </summary>
        [Description("ftps://")]
        Ftps = 0x09,

        /// <summary>
        /// sftp://
        /// </summary>
        [Description("sftp://")]
        Sftp = 0x0A,

        /// <summary>
        /// smb://
        /// </summary>
        [Description("smb://")]
        Smb = 0x0B,

        /// <summary>
        /// nfs://
        /// </summary>
        [Description("nfs://")]
        Nfs = 0x0C,

        /// <summary>
        /// ftp://
        /// </summary>
        [Description("ftp://")]
        Ftp = 0x0D,

        /// <summary>
        /// dav://
        /// </summary>
        [Description("dav://")]
        Dav = 0x0E,

        /// <summary>
        /// news:
        /// </summary>
        [Description("news:")]
        News = 0x0F,

        /// <summary>
        /// telnet://
        /// </summary>
        [Description("telnet://")]
        Telnet = 0x10,

        /// <summary>
        /// imap:
        /// </summary>
        [Description("imap:")]
        Imap = 0x11,

        /// <summary>
        /// rtsp://
        /// </summary>
        [Description("rtsp://")]
        Rtsp = 0x12,

        /// <summary>
        /// urn:
        /// </summary>
        [Description("urn:")]
        Urn = 0x13,

        /// <summary>
        /// pop:
        /// </summary>
        [Description("pop:")]
        Pop = 0x14,

        /// <summary>
        /// sip:
        /// </summary>
        [Description("sip:")]
        Sip = 0x15,

        /// <summary>
        /// sips:
        /// </summary>
        [Description("sips:")]
        Sips = 0x16,

        /// <summary>
        /// tftp:
        /// </summary>
        [Description("tftp:")]
        Tftp = 0x17,

        /// <summary>
        /// btspp://
        /// </summary>
        [Description("btspp://")]
        Btspp = 0x18,

        /// <summary>
        /// btl2cap://
        /// </summary>
        [Description("btl2cap://")]
        Btl2Cap = 0x19,

        /// <summary>
        /// btgoep://
        /// </summary>
        [Description("btgoep://")]
        Btgoep = 0x1A,

        /// <summary>
        /// tcpobex://
        /// </summary>
        [Description("tcpobex://")]
        Tcpobex = 0x1B,

        /// <summary>
        /// irdaobex://
        /// </summary>
        [Description("irdaobex://")]
        Irdaobex = 0x1C,

        /// <summary>
        /// file://
        /// </summary>
        [Description("file://")]
        File = 0x1D,

        /// <summary>
        /// urn:epc:id:
        /// </summary>
        [Description("urn:epc:id:")]
        UrnEpcId = 0x1E,

        /// <summary>
        /// urn:epc:tag:
        /// </summary>
        [Description("urn:epc:tag:")]
        UrnEpcTag = 0x1F,

        /// <summary>
        /// urn:epc:pat:
        /// </summary>
        [Description("urn:epc:pat:")]
        UrnEpcPat = 0x20,

        /// <summary>
        /// urn:epc:raw:
        /// </summary>
        [Description("urn:epc:raw:")]
        UrnEpcRaw = 0x21,

        /// <summary>
        /// urn:epc:
        /// </summary>
        [Description("urn:epc:")]
        UrnEpc = 0x22,

        /// <summary>
        /// urn:nfc:
        /// </summary>
        [Description("urn:nfc:")]
        UrnNfc = 0x23,
    }
}
