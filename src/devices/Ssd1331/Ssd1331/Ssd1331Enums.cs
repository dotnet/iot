using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Ssd1331
{
    public enum FontSize
    {
        Normal = 0,
        Wide = 1,
        High = 2,
        WideHigh = 3,
        WideHighX36 = 4
    }

    public enum FrameInterval
    {
        Frames6 = 0,
        Framesd10 = 1,
        Frames100 = 2,
        Frames200 = 3
    }

    /// <summary>
    /// Horizontal or Vertical Address Increment
    /// </summary>
    public enum AddressIncrement
    {
        Horizontal = 0,
        Vertical = 1
    }

    /// <summary>
    /// SEG0 common
    /// </summary>
    public enum Seg0Common
    {
        /// <summary>Column 0</summary>
        Column0 = 0x00,

        /// <summary>Column 95</summary>
        Column95 = 0x01
    }

    /// <summary>
    /// Color sequence
    /// </summary>
    public enum ColorSequence
    {
        /// <summary>RGB (red, green, blue)</summary>
        RGB = 0x00,

        /// <summary>BGR (blue, green, red)</summary>
        BGR = 0x01

    }

    public enum RightLeftSwapping
    {
        Disable = 0,
        Enable = 1
    }

    public enum ScanMode
    {
        FromColumn0 = 0,
        ToColumn0 = 1
    }


    /// <summary>
    /// Common split
    /// </summary>
    public enum CommonSplit
    {
        /// <summary>None</summary>
        None = 0x00,

        /// <summary>Parity split (odd and even numbers)</summary>
        OddEven = 0x01
    }

    /// <summary>
    /// Color depth
    /// </summary>
    public enum ColorDepth
    {
        /// <summary>Color depth: 256</summary>
        ColourDepth256 = 0x00,

        /// <summary>Color depth: 65k</summary>
        ColourDepth65K = 0x01,

        /// <summary>Color depth: 65k format 2</summary>
        ColourDepth65K2 = 0x02
    }

    /// <summary>
    /// High voltage level (VCOMH) of common pins relative to VCC
    /// </summary>
    public enum VComHDeselectLevel
    {
        /// <summary>0.44 of VCC level</summary>
        VccX044 = 0x00,

        /// <summary>0.52 of VCC level</summary>
        VccX052 = 0x10,

        /// <summary>0.61 of VCC level</summary>
        VccX061 = 0x20,

        /// <summary>0.71 of VCC level</summary>
        VccX071 = 0x30,

        /// <summary>0.83 of VCC level</summary>
        VccX083 = 0x3E
    }

}
