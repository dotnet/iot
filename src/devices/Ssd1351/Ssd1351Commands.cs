// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Iot.Device.Ssd1351
{
    internal enum Ssd1351Command : byte
    {
        SetColumn = 0x15,
        WriteRam = 0x5C,
        SetRow = 0x75,
        SetHorizontalScroll = 0x96,
        StopScroll = 0x9E,
        StartScroll = 0x9F,
        SetRemap = 0xA0,
        SetStartLine = 0xA1,
        SetDisplayOffset = 0xA2,
        SetDisplayAllOff = 0xA4,
        SetDisplayAllOn = 0xA5,
        SetNormalDisplay = 0xA6,
        SetInvertedDisplay = 0xA7,
        SelectFunction = 0xAB,
        SetDisplayOff = 0xAE,
        SetDisplayOn = 0xAF,
        SetPrecharge = 0xB1,
        SetDisplayEnhancement = 0xB2,
        SetClockDiv = 0xB3,
        SetVSL = 0xB4,
        SetGPIO = 0xB5,
        SetPrecharge2 = 0xB6,
        SetGrayLevels = 0xB8,
        SetDefaultGrayLevels = 0xB9,
        SetPrechargeVoltageLevel = 0xBB,
        SetDeselectVoltageLevel = 0xBE,
        SetContrastABC = 0xC1,
        SetContrastMasterCurrent = 0xC7,
        SetMultiplexorRatio = 0xCA,
        SetCommandLocks = 0xFD,

        ReadRam = 0x5D, // Note that this is not used with an SPI interface as the SSD1351 cannot be read
    }

    /// <summary>
    /// Gpio mode
    /// </summary>
    public enum GpioMode
    {
        /// <summary>Disabled</summary>
        Disabled = 0x00,

        /// <summary>Input enabled</summary>
        InputEnabled = 0x01,

        /// <summary>Output low</summary>
        OutputLow = 0x10,

        /// <summary>Output high</summary>
        OutputHigh = 0x11
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

        /// <summary>Color depth: 262k</summary>
        ColourDepth262K = 0x02,

        /// <summary>Color depth: 262k 16-bit</summary>
        ColourDepth262K16Bit = 0x03
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
    /// SEG0 common
    /// </summary>
    public enum Seg0Common
    {
        /// <summary>Column 0</summary>
        Column0 = 0x00,

        /// <summary>Column 127</summary>
        Column127 = 0x01
    }

    /// <summary>
    /// Color sequence
    /// </summary>
    public enum ColorSequence
    {
        /// <summary>BGR (blue, green, red)</summary>
        BGR = 0x00,

        /// <summary>RGB (red, green, blue)</summary>
        RGB = 0x01
    }

    /// <summary>
    /// High voltage level (VCOMH) of common pins relative to VCC
    /// </summary>
    public enum VComHDeselectLevel
    {
        /// <summary>0.72 of VCC level</summary>
        VccX072 = 0x00,

        /// <summary>0.74 of VCC level</summary>
        VccX074 = 0x01,

        /// <summary>0.76 of VCC level</summary>
        VccX076 = 0x02,

        /// <summary>0.78 of VCC level</summary>
        VccX078 = 0x03,

        /// <summary>0.80 of VCC level</summary>
        VccX080 = 0x04,

        /// <summary>0.82 of VCC level</summary>
        VccX082 = 0x05,

        /// <summary>0.84 of VCC level</summary>
        VccX084 = 0x06,

        /// <summary>0.86 of VCC level</summary>
        VccX086 = 0x07
    }

    /// <summary>
    /// Source of VDD
    /// </summary>
    public enum VDDSource
    {
        /// <summary>External VDD source</summary>
        External = 0x00,

        /// <summary>Internal VDD source</summary>
        Internal = 0x01
    }

    /// <summary>
    /// Horizontal scroll direction
    /// </summary>
    public enum ScrollDirection : byte
    {
        /// <summary>No scroll</summary>
        NoScroll = 0x00,

        /// <summary>Scroll to segment 127</summary>
        Scroll2Seg127 = 0x01,

        /// <summary>Scroll to segment 0</summary>
        Scroll2Seg0 = 0x40
    }

    /// <summary>
    /// Horizontal scroll speed
    /// </summary>
    public enum ScrollSpeed : byte
    {
        /// <summary>Normal speed</summary>
        Normal = 0x01,

        /// <summary>Slow speed</summary>
        Slow = 0x02,

        /// <summary>Slowest speed</summary>
        Slowest = 0x03
    }

    /// <summary>
    /// Constructs Ssd1351 instance
    /// </summary>
    public partial class Ssd1351 : IDisposable
    {
        /// <summary>
        /// This command is used to lock the OLED driver IC from accepting any command except itself.
        /// </summary>
        public void Lock()
        {
            SendCommand(Ssd1351Command.SetCommandLocks, 0x16);
        }

        /// <summary>
        /// This command allows the driver IC to resume from the “Lock” state. And the driver IC will then respond to the command and memory access.
        /// </summary>
        public void Unlock()
        {
            SendCommand(Ssd1351Command.SetCommandLocks, 0x12);
        }

        /// <summary>
        ///  Make commands A2,B1,B3,BB,BE,C1 accessible in the unlocked state
        /// </summary>
        public void MakeAccessible()
        {
            SendCommand(Ssd1351Command.SetCommandLocks, 0xB1);
        }

        /// <summary>
        ///  Make commands A2,B1,B3,BB,BE,C1 inaccessible in both lock and unlock state
        /// </summary>
        public void MakeInaccessible()
        {
            SendCommand(Ssd1351Command.SetCommandLocks, 0xB0);
        }

        /// <summary>
        /// This command turns the OLED panel display on.
        /// </summary>
        public void SetDisplayOn()
        {
            SendCommand(Ssd1351Command.SetDisplayOn);
        }

        /// <summary>
        /// This command turns the OLED panel display off.
        /// </summary>
        public void SetDisplayOff()
        {
            SendCommand(Ssd1351Command.SetDisplayOff);
        }

        /// <summary>
        /// This command enhances display performance.
        /// </summary>
        /// <param name="enhanceDisplay">When set to true turns on enhanced display mode. (defaults to not enhanced)</param>
        public void SetDisplayEnhancement(bool enhanceDisplay = false)
        {
            SendCommand(Ssd1351Command.SetDisplayEnhancement, (byte)(enhanceDisplay ? 0xA4 : 0x00), 0x00, 0x00);
        }

        /// <summary>
        /// This double byte command is used to set the phase 3 second pre-charge period.  The period of phase 3 is ranged from 1 to 15 DCLK's.
        /// </summary>
        /// <param name="phase3Period">Phase 3 period with a range of 1-15. (defaults to 8 DCLKs)</param>
        public void Set3rdPreChargePeriod(byte phase3Period = 0x08)
        {
            if (!Ssd1351.InRange(phase3Period, 0x01, 0x0F))
            {
                throw new ArgumentException("The phase 3 period is invalid.", nameof(phase3Period));
            }

            SendCommand(Ssd1351Command.SetPrecharge2, phase3Period);
        }

        /// <summary>
        /// This double byte command is used to set the pre-charge voltage level. The precharge
        /// voltage level ranges from 0.20 x Vcc -> 0.60 x Vcc.
        /// </summary>
        /// <param name="prechargeLevel">Pre-charge voltage level with a range of 0-31 that represents 0.20 x Vcc -> 0.60 x Vcc. (defaults to 0.38 x Vcc)</param>
        public void SetPreChargeVoltageLevel(byte prechargeLevel = 0x17)
        {
            if (!Ssd1351.InRange(prechargeLevel, 0x00, 0x1F))
            {
                throw new ArgumentException("The pre-charge voltage level is invalid.", nameof(prechargeLevel));
            }

            SendCommand(Ssd1351Command.SetPrechargeVoltageLevel, prechargeLevel);
        }

        /// <summary>
        /// This triple byte command specifies column start address and end address of the display data RAM.
        /// This command also sets the column address pointer to column start address. This pointer is used
        /// to define the current read/write column address in graphic display data RAM. If horizontal address
        /// increment mode is enabled by command 20h, after finishing read/write one column data, it is
        /// incremented automatically to the next column address. Whenever the column address pointer finishes
        /// accessing the end column address, it is reset back to start column address and the row address
        /// is incremented to the next row.  This command is only for horizontal or vertical addressing modes.
        /// </summary>
        /// <param name="startColumn">Column start address with a range of 0-127. (defaults to 0)</param>
        /// <param name="endColumn">Column end address with a range of 0-127. (defaults to 127)</param>
        public void SetColumnAddress(byte startColumn = 0x00, byte endColumn = 0x7F)
        {
            if (startColumn > 0x7F)
            {
                throw new ArgumentException("The column start address is invalid.", nameof(startColumn));
            }

            if (endColumn > 0x7F)
            {
                throw new ArgumentException("The column end address is invalid.", nameof(endColumn));
            }

            if (endColumn < startColumn)
            {
                throw new ArgumentException("The column end address must be greater or equal to the row start address.", nameof(endColumn));
            }

            SendCommand(Ssd1351Command.SetColumn, startColumn, endColumn);
        }

        /// <summary>
        /// This triple byte command specifies row start address and end address of the display
        /// data RAM.This command also sets the row address pointer to row start address.This
        /// pointer is used to define the current read/write row address in graphic display data
        /// RAM. If vertical address increment mode is enabled by command A0h, after finishing
        /// read/write one row data, it is incremented automatically to the next row address.
        /// Whenever the row address pointer finishes accessing the end row address, it is
        /// reset back to start row address.
        /// </summary>
        /// <param name="startRow">Row start address with a range of 0-127. (defaults to 0)</param>
        /// <param name="endRow">Row end address with a range of 0-127. (defaults to 127)</param>
        public void SetRowAddress(byte startRow = 0x00, byte endRow = 0x7F)
        {
            if (startRow > 0x7F)
            {
                throw new ArgumentException("The row start address is invalid.", nameof(startRow));
            }

            if (endRow > 0x7F)
            {
                throw new ArgumentException("The row end address is invalid.", nameof(endRow));
            }

            if (endRow < startRow)
            {
                throw new ArgumentException("The row end address must be greater or equal to the row start address.", nameof(endRow));
            }

            SendCommand(Ssd1351Command.SetRow, startRow, endRow);
        }

        /// <summary>
        /// This command sets the divide ratio to generate DCLK (Display Clock) from CLK and
        /// programs the oscillator frequency Fosc that is the source of CLK if CLS pin is pulled high.
        /// </summary>
        /// <param name="displayClockDivideRatio">Display clock divide ratio with a range of 0-15. (defaults to 1)</param>
        /// <param name="oscillatorFrequency">Oscillator frequency with a range of 0-15. (defaults to 13)</param>
        public void SetDisplayClockDivideRatioOscillatorFrequency(byte displayClockDivideRatio = 0x01, byte oscillatorFrequency = 0x0D)
        {
            if (displayClockDivideRatio > 0x0F)
            {
                throw new ArgumentException("The display clock divide ratio is invalid.", nameof(displayClockDivideRatio));
            }

            if (oscillatorFrequency > 0x0F)
            {
                throw new ArgumentException("The oscillator frequency is invalid.", nameof(oscillatorFrequency));
            }

            SendCommand(Ssd1351Command.SetClockDiv, (byte)((oscillatorFrequency << 4) | displayClockDivideRatio));
        }

        /// <summary>
        /// This command is used to set Contrast Setting of the display.
        /// The chip has 256 contrast steps from 00h to FFh.  The segment
        /// output current ISEG increases linearly with the contrast step,
        /// which results in brighter display.
        /// </summary>
        /// <param name="colorAContrast">Contrast level for color A. (defaults to 0x86)</param>
        /// <param name="colorBContrast">Contrast level for color B. (defaults to 0x51)</param>
        /// <param name="colorCContrast">Contrast level for color C. (defaults to 0x86)</param>
        public void SetContrastABC(byte colorAContrast = 0x86, byte colorBContrast = 0x51, byte colorCContrast = 0x86)
        {
            SendCommand(Ssd1351Command.SetContrastABC, colorAContrast, colorBContrast, colorCContrast);
        }

        /// <summary>
        /// This command specifies the mapping of the display start line to one of COM0-COM127
        /// (assuming that COM0 is the display start line then the display start line register is equal to 0).
        /// </summary>
        /// <param name="displayOffset">Display offset with a range of 0-127. (defaults to 0x60)</param>
        public void SetDisplayOffset(byte displayOffset = 0x60)
        {
            if (displayOffset > 0x7F)
            {
                throw new ArgumentException("The display offset is invalid.", nameof(displayOffset));
            }

            SendCommand(Ssd1351Command.SetDisplayOffset, displayOffset);
        }

        /// <summary>
        /// This command sets the Display Start Line register to determine starting address of display RAM,
        /// by selecting a value from 0 to 127. With value equal to 0, RAM row 0 is mapped to COM0.
        /// With value equal to 1, RAM row 1 is mapped to COM0 and so on.
        /// </summary>
        /// <param name="displayStartLine">Display start line with a range of 0-127. (defaults to 0)</param>
        public void SetDisplayStartLine(byte displayStartLine = 0x00)
        {
            if (displayStartLine > 0x7F)
            {
                throw new ArgumentException("The display start line is invalid.", nameof(displayStartLine));
            }

            SendCommand(Ssd1351Command.SetStartLine, displayStartLine);
        }

        /// <summary>
        /// This double byte command is used to set the states of GPIO0 and GPIO1 pins
        /// </summary>
        /// <param name="pin0Mode">The GpioMode of Pin0. (defaults to Output/Low)</param>
        /// <param name="pin1Mode">The GpioMode of Pin1. (defaults to Output/Low)</param>
        public void SetGpio(GpioMode pin0Mode = GpioMode.OutputLow, GpioMode pin1Mode = GpioMode.OutputLow)
        {
            SendCommand(Ssd1351Command.SetGPIO, (byte)(((int)pin1Mode << 2) + pin0Mode));
        }

        /// <summary>
        /// This command sets the gray levels GS0 -> GS63.
        /// </summary>
        /// <param name="grayLevels">A byte array containing 64 gray levels representing GS0 -> GS63.
        /// If this paramneter is null or an empty array then the gray leves are set to default.</param>
        public void SetGrayLevels(byte[] grayLevels = null)
        {
            if (grayLevels == null || grayLevels.Length == 0)
            {
                SendCommand(Ssd1351Command.SetDefaultGrayLevels);
            }
            else
            {
                if (grayLevels.Length != 64)
                {
                    throw new ArgumentException("The gray level array must contain 64 entries.", nameof(grayLevels));
                }

                SendCommand(Ssd1351Command.SetGrayLevels, grayLevels);
            }
        }

        /// <summary>
        /// This command sets the display to be inverse.
        /// The gray level of display data are swapped such that “GS0” ↔ “GS63”, “GS1” ↔ “GS62”
        /// </summary>
        public void SetInverseDisplay()
        {
            SendCommand(Ssd1351Command.SetInvertedDisplay);
        }

        /// <summary>
        /// This command sets the display to be normal where the display reflects the contents of the RAM..
        /// </summary>
        public void SetNormalDisplay()
        {
            SendCommand(Ssd1351Command.SetNormalDisplay);
        }

        /// <summary>
        /// This command sets the display to have all pixels at GS0.
        /// </summary>
        public void SetDisplayAllOff()
        {
            SendCommand(Ssd1351Command.SetDisplayAllOff);
        }

        /// <summary>
        /// This command sets the display to have all pixels at GS63.
        /// </summary>
        public void SetDisplayAllOn()
        {
            SendCommand(Ssd1351Command.SetDisplayAllOn);
        }

        /// <summary>
        /// This double byte command is to control the segment output current by a scaling factor.
        /// The chip has 16 master control steps, with the factor ranges from 1 [0000b] to 16
        /// [1111b – default]. The smaller the master current value, the dimmer the OLED panel
        /// display is set.   For example, if original segment output current is 160uA at
        /// scale factor = 16, setting scale factor to 8 would reduce the current to 80uA
        /// </summary>
        /// <param name="masterContrast">Master Contrast 0 -> 15.(defaults to 15)</param>
        public void SetMasterContrast(byte masterContrast = 0x0F)
        {
            if (!Ssd1351.InRange(masterContrast, 0x00, 0x0F))
            {
                throw new ArgumentException("The master contrast is invalid.", nameof(masterContrast));
            }

            SendCommand(Ssd1351Command.SetContrastMasterCurrent, masterContrast);
        }

        /// <summary>
        /// This command switches the default 63 multiplex mode to any multiplex ratio, ranging from 15 to 127.
        /// The output pads COM0-COM127 will be switched to the corresponding COM signal.
        /// </summary>
        /// <param name="multiplexRatio">Multiplex ratio with a range of 15-127. (defaults to 127)</param>
        public void SetMultiplexRatio(byte multiplexRatio = 127)
        {
            if (!Ssd1351.InRange(multiplexRatio, 0x0F, 0x7F))
            {
                throw new ArgumentException("The multiplex ratio is invalid.", nameof(multiplexRatio));
            }

            SendCommand(Ssd1351Command.SetMultiplexorRatio, multiplexRatio);
        }

        /// <summary>
        /// This double byte command sets the length of phase 1 and 2 of segment waveform of the driver.
        /// Phase 1: Set the period from 5 to 31 in the unit of 2 DCLKs.  A larger capacitance of the
        /// OLED pixel may require longer period to discharge the previous data charge completely.
        /// Phase 2 (A[7:4]): Set the period from 3 to 15 in the unit of DCLKs.  A longer period
        /// is needed to charge up a larger capacitance of the OLED pixel to the target voltage.
        /// </summary>
        /// <param name="phase1Period">Phase 1 period with a range of 2-15. (defaults to 2 x 2 DCLKs)</param>
        /// <param name="phase2Period">Phase 2 period with a range of 3-15. (defaults to 8 DCLKs)</param>
        public void SetPreChargePeriods(byte phase1Period = 0x02, byte phase2Period = 0x08)
        {
            if (!Ssd1351.InRange(phase1Period, 0x02, 0x0F))
            {
                throw new ArgumentException("The phase 1 period is invalid.", nameof(phase1Period));
            }

            if (!Ssd1351.InRange(phase2Period, 0x03, 0x0F))
            {
                throw new ArgumentException("The phase 2 period is invalid.", nameof(phase2Period));
            }

            SendCommand(Ssd1351Command.SetPrecharge, (byte)((phase2Period << 4) | phase1Period));
        }

        /// <summary>
        /// This command changes the mapping between the display data column address and the segment driver.
        /// It allows flexibility in OLED module design. This command only affects subsequent data input.
        /// Data already stored in GDDRAM will have no changes.
        /// </summary>
        /// <param name="colorDepth">Number of colors displayed. (defaults to 0x65K)</param>
        /// <param name="commonSplit">Defines if to split commons odd then even columns. (defaults to odd/even)</param>
        /// <param name="seg0Common">Column address 0 is mapped to SEG0 when set to Column0. Column address 127 is mapped to SEG0 when set to Column127. (defaults to Segment0 = Column0)</param>
        /// <param name="colorSequence">Colors are ordered R->G->B when set to RGB. Colors are ordered B->G->A when set to BGR. (defaults to BGR)</param>
        public void SetSegmentReMapColorDepth(ColorDepth colorDepth = ColorDepth.ColourDepth65K, CommonSplit commonSplit = CommonSplit.OddEven, Seg0Common seg0Common = Seg0Common.Column0, ColorSequence colorSequence = ColorSequence.BGR)
        {
            if (colorDepth == ColorDepth.ColourDepth262K16Bit)
            {
                throw new ArgumentException("Color depth 262k format 2 is not supported via the SPI interface.", nameof(colorDepth));
            }

            if (colorDepth == ColorDepth.ColourDepth256)
            {
                throw new ArgumentException("Color depth 256 not supported.", nameof(colorDepth));
            }

            SendCommand(Ssd1351Command.SetRemap, (byte)(((int)colorDepth << 6) + ((int)commonSplit << 5) + ((int)seg0Common << 4) + ((int)colorSequence << 2)));
        }

        /// <summary>
        /// This double byte command sets the high voltage level of common pins, VCOMH.
        /// The level of VCOMH is programmed with reference to VC.
        /// </summary>
        /// <param name="level">Vcomh deselect level. (defaults to 0.82 x Vcc)</param>
        public void SetVcomhDeselectLevel(VComHDeselectLevel level = VComHDeselectLevel.VccX082)
        {
            SendCommand(Ssd1351Command.SetDeselectVoltageLevel, (byte)level);
        }

        /// <summary>
        /// This double byte command is used to enable or disable the VDD regulator
        /// </summary>
        /// <param name="vddSource">The source of VDD. (defaults to Internal)</param>
        public void SetVDDSource(VDDSource vddSource = VDDSource.Internal)
        {
            SendCommand(Ssd1351Command.SelectFunction, (byte)vddSource);
        }

        /// <summary>
        /// Set the segment voltage reference values. Note that for the Adafruit board then use the defaults
        /// (in fact not sure if you can use anything but the defaults accoring to the datasheet)
        /// </summary>
        /// <param name="vslValue0">VSL Value 0. (defaults to 0xA0)</param>
        /// <param name="vslValue1">VSL Value 1. (defaults to 0xB5)</param>
        /// <param name="vslValue2">VSL Value 2. (defaults to 0x55)</param>
        public void SetVSL(byte vslValue0 = 0xA0, byte vslValue1 = 0xB5, byte vslValue2 = 0x55)
        {
            SendCommand(Ssd1351Command.SetVSL, vslValue0, vslValue1, vslValue2);
        }

        /// <summary>
        /// Start horizontal scrolling
        /// </summary>
        public void StartScrolling()
        {
            SendCommand(Ssd1351Command.StartScroll);
        }

        /// <summary>
        /// Stop horizontal scrolling
        /// </summary>
        public void StopScrolling()
        {
            SendCommand(Ssd1351Command.StopScroll);
        }

        /// <summary>
        /// Set Horizontal Scroll
        /// </summary>
        /// <param name="scrollDirection">The direction for the horizontal scrolling to scroll</param>
        /// <param name="startRow">The first row to be scrolled</param>
        /// <param name="numberOfRows">The number od rows to be scrolled</param>
        /// <param name="scrollSpeed">The speed of the horizontal scroll</param>
        public void SetHorizontalScroll(ScrollDirection scrollDirection, byte startRow, byte numberOfRows, ScrollSpeed scrollSpeed)
        {
            if (startRow > 0x7F)
            {
                throw new ArgumentException("The row start address is invalid.", nameof(startRow));
            }

            if (numberOfRows == 0 || numberOfRows + startRow > 0x80)
            {
                throw new ArgumentException("The number of rows is invalid.", nameof(numberOfRows));
            }

            SendCommand(Ssd1351Command.SetHorizontalScroll, (byte)scrollDirection, startRow, numberOfRows, 0x00, (byte)scrollSpeed);
        }

    }

}
