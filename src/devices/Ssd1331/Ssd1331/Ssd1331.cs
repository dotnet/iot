// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Iot.Device.Ssd1331
{
    public partial class Ssd1331 : IDisposable
    {
        #region Private Variables

        private readonly int _dcPinId;
        private readonly int _resetPinId;
        private readonly bool _shouldDispose;

        private SpiDevice _spiDevice;
        private GpioController _gpioDevice;

        private Color _bgroundColor = Color.Black;
        private Color _charColor = Color.White;
        private int _charX;
        private int _charY;
        private FontSize _chrSize = FontSize.Normal;
        private bool _externalfont;
        private byte[]? _font;
        private byte _x;
        private byte _y;
        private byte _x1;
        private byte _x2;
        private byte _y1;
        private byte _y2;

        #endregion

        #region Constructor and Disposable Implementation

        /// <summary>
        /// Initializes new instance of Ssd1331 device that will communicate using SPI bus.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication. This Spi device will be displed along with the Ssd1331 device.</param>
        /// <param name="gpioController">The GPIO controller used for communication and controls the the <paramref name="resetPin"/> and the <paramref name="dataCommandPin"/>
        /// If no Gpio controller is passed in then a default one will be created and disposed when Ssd1351 device is disposed.</param>
        /// <param name="dataCommandPin">The id of the GPIO pin used to control the DC line (data/command).</param>
        /// <param name="resetPin">The id of the GPIO pin used to control the /RESET line (data/command).</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Ssd1331(SpiDevice spiDevice, int dataCommandPin, int resetPin = -1, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _gpioDevice = gpioController ?? new GpioController();
            _shouldDispose = shouldDispose || gpioController is null;

            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            if (dataCommandPin < 0)
                throw new ArgumentException("Data Command Pin must be a positive number", nameof(dataCommandPin));

            _dcPinId = dataCommandPin;
            _resetPinId = resetPin;

            _gpioDevice.OpenPin(_dcPinId, PinMode.Output);

            if (resetPin > 0)
                _gpioDevice.OpenPin(_resetPinId, PinMode.Output);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _gpioDevice.ClosePin(_dcPinId);
            if (_resetPinId > 0)
                _gpioDevice.ClosePin(_resetPinId);

            if (_shouldDispose)
            {
                _gpioDevice?.Dispose();
                _gpioDevice = null!;
            }


            _spiDevice?.Dispose();
            _spiDevice = null!;
        }

        #endregion

        #region Private Support Methods

        /// <summary>
        /// Write a block of data to the SPI device
        /// </summary>
        /// <param name="data">The data to be sent to the SPI device</param>
        /// <param name="blnIsCommand">A flag indicating that the data is really a command when true or data when false.</param>
        private void SendSPI(Span<byte> data, bool blnIsCommand = false)
        {
            // set the DC pin to indicate if the data being sent to the display is DATA or COMMAND bytes.
            _gpioDevice.Write(_dcPinId, blnIsCommand ? PinValue.Low : PinValue.High);
            _spiDevice.Write(data);
        }

        /// <summary>
        /// Write a block of data to the SPI device
        /// </summary>
        /// <param name="data">The data to be sent to the SPI device</param>
        /// <param name="blnIsCommand">A flag indicating that the data is really a command when true or data when false.</param>
        private void SendSPI(ReadOnlySpan<byte> data, bool blnIsCommand = false)
        {
            // set the DC pin to indicate if the data being sent to the display is DATA or COMMAND bytes.
            _gpioDevice.Write(_dcPinId, blnIsCommand ? PinValue.Low : PinValue.High);
            _spiDevice.Write(data);
        }

        /// <summary>
        /// Verifies value is within a specific range.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="start">Starting value of range.</param>
        /// <param name="end">Ending value of range.</param>
        /// <returns>Determines if value is within range.</returns>
        private static bool InRange(uint value, uint start, uint end)
        {
            return value >= start && value <= end;
        }

        private (byte C, byte B, byte A) ToRGB(Color col)
        {
            ushort c;
            c = (ushort)(col.R >> 3);
            c <<= 6;
            c |= (ushort)(col.G >> 2);
            c <<= 5;
            c |= (ushort)(col.B >> 3);

            byte lC = (byte)((c >> 11) << 1);
            byte lB = (byte)((c >> 5) & 0x3F);
            byte lA = (byte)((c << 1) & 0x3F);

            return (lC, lB, lA);
        }

        private (byte high, byte low) ToHighLow(Color col)
        {
            ushort c;
            c = (ushort)(col.R >> 3);
            c <<= 6;
            c |= (ushort)(col.G >> 2);
            c <<= 5;
            c |= (ushort)(col.B >> 3);

            byte high = (byte)(c >> 8);
            byte low = (byte)(c & 0xFF);

            return (high, low);
        }

        private void MaxSsd1331Window()
        {
            SetColumnAddress();
            SetRowAddress();
        }

        private void putp(Color color)
        {
            Pixel(_x, _y, color);
            _x++;
            if (_x > _x2)
            {
                _x = _x1;
                _y++;
                if (_y > _y2)
                {
                    _y = _y1;
                }
            }
        }

        #endregion

        #region Public Communication Methods

        /// <summary>
        /// Send a command to the the display controller along with associated parameters.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="commandParameters">parameteters for the command to be sent</param>
        public void SendCommand(byte command, params byte[] commandParameters)
        {
            Span<byte> paramSpan = new Span<byte>(commandParameters);

            SendCommand(command, paramSpan);
        }

        /// <summary>
        /// Send a command to the the display controller along with parameters.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="data">Span to send as parameters for the command.</param>
        public void SendCommand(byte command, Span<byte> data)
        {
            if (data == null || data.Length == 0)
            {
                Span<byte> commandSpan = stackalloc byte[]
                {
                    command
                };

                SendSPI(commandSpan, true);
            }
            else
            {
                Span<byte> commandSpan = stackalloc byte[data.Length + 1];
                commandSpan[0] = command;
                for (int i = 0; i < data.Length; i++)
                {
                    commandSpan[i + 1] = data[i];
                }

                SendSPI(commandSpan, true);
            }
        }

        /// <summary>
        /// Send data to the the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public void SendData(params byte[] data)
        {
            Span<byte> paramSpan = new Span<byte>(data);

            SendData(paramSpan);
        }

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public void SendData(Span<byte> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            SendSPI(data);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Resets the display.
        /// </summary>
        public void Reset()
        {
            if (_resetPinId > 0)
            {
                _gpioDevice.Write(_dcPinId, PinValue.Low);
                _gpioDevice.Write(_resetPinId, PinValue.High);

                Task.Delay(20).Wait();

                _gpioDevice.Write(_resetPinId, PinValue.Low);
                Task.Delay(20).Wait();

                _gpioDevice.Write(_resetPinId, PinValue.High);
                Task.Delay(20).Wait();
            }
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        public void Initialize()
        {
            Lock(false);  // Unlock OLED driver IC MCU interface from entering command
            SetDisplayOff(); // Turn on sleep mode
            SetRowAddress(); // Rows = 0 -> 63
            SetColumnAddress(); // Columns = 0 -> 95
            SetSegmentReMapColorDepth(); // 0x72 Color Depth = 64K, Enable COM Split Odd Even, Scan from COM[N-1] to COM0. Where N is the Multiplex ratio., Color sequence is normal: R -> G -> B
            SetDisplayStartLine(); // set startline to to 0
            SetDisplayOffset(); // Set vertical scroll by Row to 0-63.
            SetNormalDisplay();
            SetMultiplexRatio(); // Use all 53 common lines by default....
            SetMasterConfiguration();
            EnablePowerSave(true);
            PhasePeriodAdjustment();
            SetDisplayClockDivideRatioOscillatorFrequency(); // 7:4 = Oscillator Frequency, 3:0 = CLK Div Ratio (A[3:0]+1 = 1..16)
            SetSecondPrecharge(0x81, 0x82, 0x83);
            SetPrechargeLevel();
            SetVcomhDeselectLevel();
            MasterCurrentControl();
            SetContrastColorA();
            SetContrastColorB();
            SetContrastColorC();
            SetDisplayOn();
            ClearScreen();
        }

        #endregion

        #region SSD1331 Commands

        /// <summary>
        /// Setup Column start and end address. 0x15
        /// </summary>
        /// <param name="startColumn">Column start address with a range of 0-95. (defaults to 0)</param>
        /// <param name="endColumn">Column end address with a range of 0-95. (defaults to 95)</param>
        public void SetColumnAddress(byte startColumn = 0x00, byte endColumn = _width)
        {
            if (startColumn > _width)
            {
                throw new ArgumentException("The column start address is invalid.", nameof(startColumn));
            }

            if (endColumn > _width)
            {
                throw new ArgumentException("The column end address is invalid.", nameof(endColumn));
            }

            if (endColumn < startColumn)
            {
                throw new ArgumentException("The column end address must be greater or equal to the row start address.", nameof(endColumn));
            }

            SendCommand(ssd1331SetColumnAddress, startColumn, endColumn);
        }

        /// <summary>
        /// Setup Row start and end address. 0x75
        /// </summary>
        /// <param name="startRow">Row start address with a range of 0-63. (defaults to 0)</param>
        /// <param name="endRow">Row end address with a range of 0-63. (defaults to 63)</param>
        public void SetRowAddress(byte startRow = 0x00, byte endRow = _height)
        {
            if (startRow > _height)
            {
                throw new ArgumentException("The row start address is invalid.", nameof(startRow));
            }

            if (endRow > _height)
            {
                throw new ArgumentException("The row end address is invalid.", nameof(endRow));
            }

            if (endRow < startRow)
            {
                throw new ArgumentException("The row end address must be greater or equal to the row start address.", nameof(endRow));
            }

            SendCommand(ssd1331SetRowAddress, startRow, endRow);
        }

        /// <summary>
        /// Set contrast for all color "A" segment. 0x81
        /// </summary>
        /// <param name="contrast">Contrast level 0-255. Defaults to 128</param>
        public void SetContrastColorA(byte contrast = 0x80)
        {
            SendCommand(ssd1331SetContrastA, contrast);
        }

        /// <summary>
        /// Set contrast for all color "B" segment. 0x82
        /// </summary>
        /// <param name="contrast">Contrast level 0-255. Defaults to 128</param>
        public void SetContrastColorB(byte contrast = 0x80)
        {
            SendCommand(ssd1331SetContrastB, contrast);
        }

        /// <summary>
        /// Set contrast for all color "C" segment. 0x83
        /// </summary>
        /// <param name="contrast">Contrast level 0-255. Defaults to 128</param>
        public void SetContrastColorC(byte contrast = 0x80)
        {
            SendCommand(ssd1331SetContrastC, contrast);
        }

        /// <summary>
        /// Set master current attenuation factor. 0x87
        /// </summary>
        /// <param name="attenuationFactor">Attenuation Factory 0-15 corresponding to 1/16-16/16</param>
        /// <exception cref="ArgumentException"></exception>
        public void MasterCurrentControl(byte attenuationFactor = 0x0F)
        {
            if (attenuationFactor > 0x0F)
            {
                throw new ArgumentException("The master current control attenuation factor is invalid.", nameof(attenuationFactor));
            }

            SendCommand(ssd1331MasterCurrentControl, attenuationFactor);
        }

        /// <summary>
        /// Set Second Pre-charge Speed. 0x8A, 0x8B, 0x8C
        /// All 3 must be set at the same time.
        /// </summary>
        /// <param name="A">Color "A" Pre-charge Speed</param>
        /// <param name="B">Color "B" Pre-charge Speed</param>
        /// <param name="C">Color "C" Pre-charge Speed</param>
        public void SetSecondPrecharge(byte A = 0x81, byte B = 0x82, byte C = 0x83)
        {
            SendCommand(ssd1331SetSecondPrechargeA, A, ssd1331SetSecondPrechargeB, B, ssd1331SetSecondPrechargeC, C);
        }

        /// <summary>
        /// Set driver remap and color depth. 0xA0
        /// </summary>
        /// <param name="addressIncrement">Address Increment</param>
        /// <param name="seg0Common">Column address 0 is mapped to SEG0 when set to Column0. Column address 95 is mapped to SEG0 when set to Column95. (defaults to Segment0 = Column0)</param>
        /// <param name="colorSequence">Colors are ordered R->G->B when set to RGB. Colors are ordered B->G->A when set to BGR. (defaults to RGB)</param>
        /// <param name="swapping">Enable/Disable Right/Left swapping</param>
        /// <param name="scanMode">San fdrom Column 0 to 95 or Column 95 to 0</param>
        /// <param name="commonSplit">Defines if to split commons odd then even columns. (defaults to odd/even)</param>
        /// <param name="colorDepth">Number of colors displayed. (defaults to 0x65K)</param>
        public void SetSegmentReMapColorDepth(AddressIncrement addressIncrement = AddressIncrement.Horizontal, Seg0Common seg0Common = Seg0Common.Column95, ColorSequence colorSequence = ColorSequence.RGB, LeftRightSwapping swapping = LeftRightSwapping.Disable, ScanMode scanMode = ScanMode.ToColumn0, CommonSplit commonSplit = CommonSplit.OddEven, ColorDepth colorDepth = ColorDepth.ColourDepth65K)
        {
            SendCommand(ssd1331RemapColorDepth, (byte)(((int)colorDepth << 6) + ((int)commonSplit << 5) + ((int)scanMode << 4) + ((int)swapping << 3) + ((int)colorSequence << 2) + ((int)seg0Common << 1) + addressIncrement));
        }

        /// <summary>
        /// Set display start line register by Row. 0xA1
        /// </summary>
        /// <param name="displayStartLine">Display start line with a range of 0-63. (defaults to 0)</param>
        public void SetDisplayStartLine(byte displayStartLine = 0x00)
        {
            if (displayStartLine > _height)
            {
                throw new ArgumentException("The display start line is invalid.", nameof(displayStartLine));
            }

            SendCommand(ssd1331SetDisplayStartLine, displayStartLine);
        }

        /// <summary>
        /// Set vertical offset by Com. 0xA2
        /// </summary>
        /// <param name="displayOffset">Display offset with a range of 0-63. (defaults to 0x00)</param>
        public void SetDisplayOffset(byte displayOffset = 0x00)
        {
            if (displayOffset > _height)
            {
                throw new ArgumentException("The display offset is invalid.", nameof(displayOffset));
            }

            SendCommand(ssd1331SetDisplayOffset, displayOffset);
        }

        /// <summary>
        /// Set Display Mode - Normal Display. 0xA4
        /// </summary>
        public void SetNormalDisplay()
        {
            SendCommand(ssd1331SetNormalDisplay);
        }

        /// <summary>
        /// Set Display Mode - Entire Display On. 0xA5
        /// </summary>
        public void SetDisplayAllOn()
        {
            SendCommand(ssd1331SetDisplayAllOn);
        }

        /// <summary>
        /// Set Display Mode - Entire Display Off. 0xA6
        /// </summary>
        public void SetDisplayAllOff()
        {
            SendCommand(ssd1331SetDisplayAllOff);
        }

        /// <summary>
        /// Set Display Mode - Inverse Display. 0xA7
        /// </summary>
        public void SetInverseDisplay()
        {
            SendCommand(ssd1331SetInvertedDisplay);
        }

        /// <summary>
        /// Set MUX ratio to N+1 Mux. 0xA8
        /// </summary>
        /// <param name="multiplexRatio">Multiplex ratio with a range of 15-63. (defaults to 63)</param>
        public void SetMultiplexRatio(byte multiplexRatio = _height)
        {
            if (!Ssd1331.InRange(multiplexRatio, 0x0F, _height))
            {
                throw new ArgumentException("The multiplex ratio is invalid.", nameof(multiplexRatio));
            }

            SendCommand(ssd1331SetMultiplexRatio, multiplexRatio);
        }

        /// <summary>
        /// Configure Dim Mode Setting. 0xAB
        /// </summary>
        /// <param name="colorA">Contrast for Color A</param>
        /// <param name="colorB">Contrast for Color B</param>
        /// <param name="colorC">Contrast for Color C</param>
        /// <param name="precharge">Precharge voltage setting</param>
        /// <exception cref="ArgumentException"></exception>
        public void DimModeSetting(byte colorA, byte colorB, byte colorC, byte precharge)
        {
            if (precharge > 0x1F)
            {
                throw new ArgumentException("The precharge voltage setting is invalid.", nameof(precharge));
            }

            SendCommand(ssd1331DimModeSetting, 0x00, colorA, colorB, colorC, precharge);
        }

        /// <summary>
        /// Set Master Configuration. 0xAD
        /// </summary>
        public void SetMasterConfiguration()
        {
            SendCommand(ssd1331SetMasterConfiguration, 0x8F);
        }

        /// <summary>
        /// Set Display ON in dim mode. 0xAC
        /// </summary>
        public void SetDisplayOnDim()
        {
            SendCommand(ssd1331SetDisplayOnDim);
        }

        /// <summary>
        /// Set Display OFF (sleep mode). 0xAE
        /// </summary>
        public void SetDisplayOff()
        {
            SendCommand(ssd1331SetDisplayOff);
        }

        /// <summary>
        /// Set Display ON in normal mode. 0xAF
        /// </summary>
        public void SetDisplayOn()
        {
            SendCommand(ssd1331SetDisplayOn);
        }

        /// <summary>
        /// Set Power Save mode. 0xB0
        /// 
        /// <paramref name="enable"> Enable Power Save mode</paramref>
        /// </summary>
        public void EnablePowerSave(bool enable)
        {
            SendCommand(ssd1331PowerSaveMode, (byte)(enable ? 0x1A : 0x0B));
        }

        /// <summary>
        /// Phase 1 and 2 period adjustment. 0xB1
        /// </summary>
        /// <param name="phase1Period">Phase 1 period in N DCLK, 1-15 allowed.</param>
        /// <param name="phase2Period">Phase 2 period in N DCLK, 1-15 allowed.</param>
        /// <exception cref="ArgumentException"></exception>
        public void PhasePeriodAdjustment(byte phase1Period = 0x04, byte phase2Period = 0x07)
        {
            if (!Ssd1331.InRange(phase1Period, 0x01, 0x0F))
            {
                throw new ArgumentException("The phase 1 period is invalid.", nameof(phase1Period));
            }
            if (!Ssd1331.InRange(phase2Period, 0x01, 0x0F))
            {
                throw new ArgumentException("The phase 2 period is invalid.", nameof(phase2Period));
            }

            SendCommand(ssd1331PhasePeriodAdjustment, (byte)((phase2Period << 4) | phase1Period));
        }

        /// <summary>
        /// This command sets the divide ratio to generate DCLK (Display Clock) from CLK and
        /// programs the oscillator frequency Fosc that is the source of CLK if CLS pin is pulled high. 0xB3
        /// </summary>
        /// <param name="displayClockDivideRatio">Display clock divide ratio with a range of 0-15. (defaults to 0)</param>
        /// <param name="oscillatorFrequency">Oscillator frequency with a range of 0-15. (defaults to 13)</param>
        public void SetDisplayClockDivideRatioOscillatorFrequency(byte displayClockDivideRatio = 0x00, byte oscillatorFrequency = 0x0D)
        {
            if (displayClockDivideRatio > 0x0F)
            {
                throw new ArgumentException("The display clock divide ratio is invalid.", nameof(displayClockDivideRatio));
            }

            if (oscillatorFrequency > 0x0F)
            {
                throw new ArgumentException("The oscillator frequency is invalid.", nameof(oscillatorFrequency));
            }

            SendCommand(ssd1331SetClockDiv, (byte)((oscillatorFrequency << 4) | displayClockDivideRatio));
        }

        /// <summary>
        /// This command sets the gray levels GS0 -> GS63. 0xB8 and 0xB9
        /// </summary>
        /// <param name="grayLevels">A byte array containing 32 gray levels representing GS0 -> GS63.
        /// If this parameter is null or an empty array then the gray leves are set to default.</param>
        public void SetGrayLevels(byte[]? grayLevels = null)
        {
            if (grayLevels is null || grayLevels.Length == 0)
            {
                SendCommand(ssd1331SetDefaultGrayLevels);
            }
            else
            {
                if (grayLevels.Length != 32)
                {
                    throw new ArgumentException("The gray level array must contain 32 entries.", nameof(grayLevels));
                }

                SendCommand(ssd1331SetGrayLevels, grayLevels);
            }
        }

        /// <summary>
        /// Set pre-charge voltage level. All three color
        /// share the same pre-charge voltage. 0xBB
        /// </summary>
        /// <param name="level">Voltage Level from 0x00 to 0x3E</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetPrechargeLevel(byte level = 0x3E)
        {
            if (level > 0x3E)
            {
                throw new ArgumentException("The pre-charge voltage level is invalid.", nameof(level));
            }

            SendCommand(ssd1331SetPrechargeLevel, level);
        }

        /// <summary>
        /// This double byte command sets the high voltage level of common pins, VCOMH.
        /// The level of VCOMH is programmed with reference to VC. 0xBE
        /// </summary>
        /// <param name="level">Vcomh deselect level. (defaults to 0.82 x Vcc)</param>
        public void SetVcomhDeselectLevel(VComHDeselectLevel level = VComHDeselectLevel.VccX083)
        {
            SendCommand(ssd1331SetVcomh, (byte)level);
        }

        /// <summary>
        /// This command is used to lock/unlock the OLED driver IC from accepting any command except itself.
        /// 
        /// <paramref name="enable">True to lock, false to unlock</paramref>
        /// </summary>
        public void Lock(bool enable)
        {
            SendCommand(ssd1331SetCommandLock, (byte)(enable ? 0x01 : 0x00));
        }

        /// <summary>
        /// Draw Line. 0x21
        /// </summary>
        /// <param name="startCol">Column Address of Start</param>
        /// <param name="startRow">Row Address of Start</param>
        /// <param name="endCol">Column Address of End</param>
        /// <param name="endRow">Row Address of End</param>
        /// <param name="colorC">Color C of the line</param>
        /// <param name="colorB">Color B of the line</param>
        /// <param name="colorA">Color A of the line</param>
        public void DrawLine(byte startCol, byte startRow, byte endCol, byte endRow, byte colorC, byte colorB, byte colorA)
        {
            SendCommand(ssd1331DrawLine, startCol, startRow, endCol, endRow, colorC, colorB, colorA);
        }

        /// <summary>
        /// Draw Rectangle. 0x22
        /// </summary>
        /// <param name="startCol">Column Address of Start</param>
        /// <param name="startRow">Row Address of Start</param>
        /// <param name="endCol">Column Address of End</param>
        /// <param name="endRow">Row Address of End</param>
        /// <param name="colorC">Color C of the line</param>
        /// <param name="colorB">Color B of the line</param>
        /// <param name="colorA">Color A of the line</param>
        /// <param name="fillColorC">Color C of the fill area</param>
        /// <param name="fillColorB">Color B of the fill area</param>
        /// <param name="fillColorA">Color A of the fill area</param>
        public void DrawRect(byte startCol, byte startRow, byte endCol, byte endRow, byte colorC, byte colorB, byte colorA, byte fillColorC = 0, byte fillColorB = 0, byte fillColorA = 0)
        {
            SendCommand(ssd1331DrawRectangle, startCol, startRow, endCol, endRow, colorC, colorB, colorA, fillColorC, fillColorB, fillColorA);
        }

        /// <summary>
        /// Copy Area. 0x23
        /// </summary>
        /// <param name="startCol">Column Address of Start</param>
        /// <param name="startRow">Row Address of Start</param>
        /// <param name="endCol">Column Address of End</param>
        /// <param name="endRow">Row Address of End</param>
        /// <param name="newStartCol">Column Address of New Start</param>
        /// <param name="newStartRow">Row Address of New Start</param>
        public void Copy(byte startCol, byte startRow, byte endCol, byte endRow, byte newStartCol, byte newStartRow)
        {
            SendCommand(ssd1331Copy, startCol, startRow, endCol, endRow, newStartCol, newStartRow);
        }

        /// <summary>
        /// Dim Window. 0x24
        /// </summary>
        /// <param name="startCol">Column Address of Start</param>
        /// <param name="startRow">Row Address of Start</param>
        /// <param name="endCol">Column Address of End</param>
        /// <param name="endRow">Row Address of End</param>
        public void DimWindow(byte startCol, byte startRow, byte endCol, byte endRow)
        {
            SendCommand(ssd1331DimWindow, startCol, startRow, endCol, endRow);
        }

        /// <summary>
        /// Clear Window
        /// </summary>
        /// <param name="startCol">Column Address of Start</param>
        /// <param name="startRow">Row Address of Start</param>
        /// <param name="endCol">Column Address of End</param>
        /// <param name="endRow">Row Address of End</param>
        public void ClearWindow(byte startCol, byte startRow, byte endCol, byte endRow)
        {
            SendCommand(ssd1331ClearWindow, startCol, startRow, endCol, endRow);
        }

        /// <summary>
        /// Enable/Disable Fill for Draw Rectangle Command. 0x26
        /// 
        /// <paramref name="enable">True to enable, False to disable</paramref>
        /// </summary>
        public void FillEnable(bool enable)
        {
            SendCommand(ssd1331FillEnable, (byte)(enable ? 1 : 0));
        }

        /// <summary>
        /// Continuous Horizontal and Vertical Scrolling Setup. 0x27
        /// </summary>
        /// <param name="horizontal">Set number of column as horizontal scroll offset. 0-95. 0 = no horizontal scroll</param>
        /// <param name="start_line">Define start row address</param>
        /// <param name="linecount">Set number of rows to be horizontal scrolled</param>
        /// <param name="vertical">Set number of row as vertical scroll offset. 0-63. 0 = no vertical scroll</param>
        /// <param name="frame_interval">Set time interval between each scroll step</param>
        public void ScrollSet(int horizontal, int start_line, int linecount, int vertical, FrameInterval frame_interval)
        {
            if ((start_line > _height + 1) || ((start_line + linecount) > _height + 1)) return;
            if (frame_interval > FrameInterval.Frames200) frame_interval = FrameInterval.Frames200;

            SendCommand(ssd1331ScrollingSetup, (byte)horizontal, (byte)start_line, (byte)linecount, (byte)vertical, (byte)frame_interval);
        }

        /// <summary>
        /// This command deactivates the scrolling action. 0x2E
        /// </summary>
        public void StopScrolling()
        {
            SendCommand(ssd1331StopScroll);
        }

        /// <summary>
        /// This command activates the scrolling function. 0x2F
        /// </summary>
        public void StartScrolling()
        {
            SendCommand(ssd1331StartScroll);
        }

        #endregion

        #region Graphics Functions

        /// <summary>
        /// Clears screen
        /// </summary>
        public void ClearScreen()
        {
            SendCommand(ssd1331ClearWindow, 0, 0, _width, _height);
            Task.Delay(100).Wait();
            MaxSsd1331Window();
            Background(Color.Black);
        }

        /// <summary>
        /// Set the Background color for further commands
        /// </summary>
        /// <param name="color">Background color</param>
        public void Background(Color color)
        {
            _bgroundColor = color;
        }

        /// <summary>
        /// Set the Foreground color for text
        /// </summary>
        /// <param name="color">Text Forground Color</param>
        public void Foreground(Color color)
        {
            _charColor = color;
        }

        public void FillScreen(Color color)
        {
            FillRect(0, 0, _width, _height, color, color);
        }

        public void Rect(byte x1, byte y1, byte x2, byte y2, Color lineColor)
        {
            if (x1 > _width) x1 = _width;
            if (y1 > _height) y1 = _height;
            if (x2 > _width) x2 = _width;
            if (y2 > _height) y2 = _height;

            byte lC, lB, lA;
            (lC, lB, lA) = ToRGB(lineColor);
            FillEnable(false);
            SendCommand(ssd1331DrawRectangle, x1, y1, x2, y2, lC, lB, lA, 0, 0, 0);
            Task.Delay(100).Wait();
        }

        /// <summary>
        /// Send filled rectangle to the ssd1351 display.
        /// </summary>
        /// <param name="color">The color to fill the rectangle with.</param>
        /// <param name="x">The x co-ordinate of the point to start the rectangle at in pixels (0..126 for the Ssd1351 where 0 represents the leftmost pixel).</param>
        /// <param name="y">The y co-ordinate of the point to start the rectangle at in pixles (0..126 for the Ssd1351 where 0 represents the topmost pixel).</param>
        /// <param name="w">The width of the rectangle in pixels (1..127 for the Ssd1351).</param>
        /// <param name="h">The height of the rectangle in pixels (1..127 for the Ssd1351).</param>
        public void FillRect(byte x, byte y, byte w, byte h, Color lineColor, Color fillColor)
        {
            byte lC, lB, lA;
            byte fC, fB, fA;
            (lC, lB, lA) = ToRGB(lineColor);
            (fC, fB, fA) = ToRGB(fillColor);

            if (x > _width)
                throw new ArgumentException("The start column is invalid.", nameof(x));
            if (x > _height)
                throw new ArgumentException("The start row is invalid.", nameof(y));
            if (x + w > _width)
                throw new ArgumentException("The width is invalid.", nameof(w));
            if (y + h > _height)
                throw new ArgumentException("The height is invalid.", nameof(h));

            FillEnable(true);
            SendCommand(ssd1331DrawRectangle, x, y, (byte)(x + w), (byte)(y + h), lC, lB, lA, fC, fB, fA);
            Task.Delay(100).Wait();
            FillEnable(false);
        }

        public void Circle(byte radius, byte x, byte y, Color col, bool fill)
        {
            if (x > _width) x = _width;
            if (y > _height) y = _height;

            int cx, cy, d;
            d = 3 - 2 * radius;
            cy = radius;
            Pixel(x, (byte)(radius + y), col);
            Pixel(x, (byte)(-radius + y), col);
            Pixel((byte)(radius + x), y, col);
            Pixel((byte)(-radius + x), y, col);
            if (fill)
            {
                Line(x, (byte)(radius + y), x, (byte)(-radius + y), col);
                Line((byte)(radius + x), y, (byte)(-radius + x), y, col);
            }

            for (cx = 0; cx <= cy; cx++)
            {
                if (d >= 0)
                {
                    d += 10 + 4 * cx - 4 * cy;
                    cy--;
                }
                else
                {
                    d += 6 + 4 * cx;
                }

                Pixel((byte)(cy + x), (byte)(cx + y), col);
                Pixel((byte)(cx + x), (byte)(cy + y), col);
                Pixel((byte)(-cx + x), (byte)(cy + y), col);
                Pixel((byte)(-cy + x), (byte)(cx + y), col);
                Pixel((byte)(-cy + x), (byte)(-cx + y), col);
                Pixel((byte)(-cx + x), (byte)(-cy + y), col);
                Pixel((byte)(cx + x), (byte)(-cy + y), col);
                Pixel((byte)(cy + x), (byte)(-cx + y), col);
                if (fill)
                {
                    Line((byte)(cy + x), (byte)(cx + y), (byte)(cy + x), (byte)(-cx + y), col);
                    Line((byte)(cx + x), (byte)(cy + y), (byte)(cx + x), (byte)(-cy + y), col);
                    Line((byte)(-cx + x), (byte)(cy + y), (byte)(-cx + x), (byte)(cy + y), col);
                    Line((byte)(-cy + x), (byte)(cx + y), (byte)(-cy + x), (byte)(cx + y), col);
                    Line((byte)(-cy + x), (byte)(-cx + y), (byte)(-cy + x), (byte)(cx + y), col);
                    Line((byte)(-cx + x), (byte)(-cy + y), (byte)(-cx + x), (byte)(cy + y), col);
                    Line((byte)(cx + x), (byte)(-cy + y), (byte)(cx + x), (byte)(cy + y), col);
                    Line((byte)(cy + x), (byte)(-cx + y), (byte)(cy + x), (byte)(cx + y), col);
                }
            }
        }

        public void Line(byte x1, byte y1, byte x2, byte y2, Color col)
        {
            if (x1 > _width) x1 = _width;
            if (y1 > _height) y1 = _height;
            if (x2 > _width) x2 = _width;
            if (y2 > _height) y2 = _height;

            byte lC, lB, lA;
            (lC, lB, lA) = ToRGB(col);
            FillEnable(false);
            SendCommand(ssd1331DrawLine, x1, y1, x2, y2, lC, lB, lA);
        }

        public void Pixel(byte x, byte y, Color col)
        {
            //byte lC, lB, lA;
            //(lC, lB, lA) = toRGB(col);

            byte low, high;
            (high, low) = ToHighLow(col);

            if (x < 0 || x > _width || y < 0 || y > _height)
                return;
            SetColumnAddress(x, x);
            SetRowAddress(y, y);
            //SendData(lC, lB, lA);
            SendData(high, low);
        }

        public void Contrast(int contrast)
        {
            int v = contrast * 20;
            if (v > 180)
                v = 180;
            SendCommand(ssd1331SetContrastA, (byte)v);
            SendCommand(ssd1331SetContrastB, (byte)v);
            SendCommand(ssd1331SetContrastC, (byte)v);
        }

        public void Window(int x, int y, int w, int h)
        {
            _x = (byte)x;
            _y = (byte)y;

            _x1 = (byte)x;
            _x2 = (byte)(x + w - 1);
            _y1 = (byte)y;
            _y2 = (byte)(y + h - 1);
            SetColumnAddress();
            SetRowAddress();
            SetColumnAddress(_x1, _x2);
            SetRowAddress(_y1, _y2);
        }

        public int Bitmap16FS(int x, int y, string Name_BMP)
        {
            int OffsetPixelWidth = 18;
            int OffsetPixelHeight = 22;
            int OffsetPixData = 10;
            int OffsetBPP = 28;
            byte[] BMP_Header = new byte[54];
            ushort BPP_t;
            int PixelWidth, PixelHeight, start_data;
            int i, off;
            int padd, j;
            byte[] line;

            FileStream Image = new FileStream(Name_BMP, FileMode.Open);
            Image.Read(BMP_Header, 0, 54);
            if (BMP_Header[0] != 0x42 || BMP_Header[1] != 0x4D)
            {
                Image.Close();
                return -1;
            }

            BPP_t = (ushort)(BMP_Header[OffsetBPP] + (BMP_Header[OffsetBPP + 1] << 8));
            if (BPP_t != 0x0010)
            {
                Image.Close();
                Console.Error.WriteLine("Error, not 16 bit BMP");
                return -2;
            }

            PixelHeight = BMP_Header[OffsetPixelHeight] + (BMP_Header[OffsetPixelHeight + 1] << 8) + (BMP_Header[OffsetPixelHeight + 2] << 16) + (BMP_Header[OffsetPixelHeight + 3] << 24);
            PixelWidth = BMP_Header[OffsetPixelWidth] + (BMP_Header[OffsetPixelWidth + 1] << 8) + (BMP_Header[OffsetPixelWidth + 2] << 16) + (BMP_Header[OffsetPixelWidth + 3] << 24);
            if (PixelHeight > _height + 1 + y || PixelWidth > _width + 1 + x)
            {
                Image.Close();
                return -3;                                        // to big
            }

            start_data = BMP_Header[OffsetPixData] + (BMP_Header[OffsetPixData + 1] << 8) + (BMP_Header[OffsetPixData + 2] << 16) + (BMP_Header[OffsetPixData + 3] << 24);

            line = new byte[PixelWidth * 2];

            padd = -1;
            do
            {
                padd++;
            } while ((PixelWidth * 2 + padd) % 4 != 0);

            Window(x, y, PixelWidth, PixelHeight);

            for (j = PixelHeight - 1; j >= 0; j--)
            {
                off = j * (PixelWidth * 2 + padd) + start_data;
                Image.Seek(off, SeekOrigin.Begin);
                Image.Read(line, 0, PixelWidth * 2);
                for (i = 0; i < PixelWidth * 2; i += 2)
                    SendData(line[i + 1], line[i]);
            }
            Image.Close();
            MaxSsd1331Window();
            return PixelWidth;
        }

        #endregion

        #region Text Functions

        public void Locate(byte column, byte row)
        {
            _charX = column;
            _charY = row;
        }

        public void Print(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            for (int ii = 0; ii < msg.Length; ii++)
                PutChar(msg[ii]);
        }

        public void PutChar(byte value)
        {
            if (_externalfont)
            {
                byte hor, vert, offset, bpl, j, i, b;
                byte z, w;
                offset = _font[0];
                hor = _font[1];
                vert = _font[2];
                bpl = _font[3];
                byte[] sign = new byte[hor * vert];
                if (value == 0x0A)
                {
                    _charX = 0;
                    _charY += vert;
                }
                if (value < 32 || value > 127) return;
                if (_charX + hor > 95)
                {
                    _charX = 0;
                    _charY += vert;
                    if (_charY >= 63 - _font[2])
                        _charY = 0;
                }

                Window(_charX, _charY, hor, vert);
                Array.Copy(_font, ((value - 32) * offset) + 4, sign, 0, vert * hor + 1);
                w = sign[0];
                for (j = 0; j < vert; j++)
                {
                    for (i = 0; i < hor; i++)
                    {
                        z = sign[bpl * i + ((j & 0xF8) >> 3) + 1];
                        b = (byte)(1 << (j & 0x07));
                        if ((z & b) == 0x00)
                            putp(_bgroundColor);
                        else
                            putp(_charColor);
                    }
                }
                if ((w + 2) < hor)
                    _charX += w + 2;
                else
                    _charX += hor;

            }
            else
            {
                if (value == 0x0A)
                {
                    _charX = 0;
                    _charY += _yHeight;
                }
                if (value < 32 || value > 127)
                    return;
                if (_charX + _xWidth > _width)
                {
                    _charX = 0;
                    _charY += _yHeight;
                    if (_charY >= _height - _yHeight)
                        _charY = 0;
                }
                int i, j, w, lpx = 0, lpy = 0, k, l, xw;
                byte Temp;
                w = _xWidth;
                FontSizeConvert(ref lpx, ref lpy);
                xw = _xWidth;

                for (i = 0; i < xw; i++)
                {
                    for (l = 0; l < lpx; l++)
                    {
                        Temp = _font6x8[value - 32, i];
                        for (j = _yHeight - 1; j >= 0; j--)
                        {
                            for (k = 0; k < lpx; k++)
                            {
                                Pixel((byte)(_charX + (i * lpx) + l), (byte)(_charY + (((j + 1) * lpy) - 1) - k), ((Temp & 0x80) == 0x80) ? _charColor : _bgroundColor);
                            }
                            Temp <<= 1;
                        }
                    }
                }
                FontSizeConvert(ref lpx, ref lpy);
                _charX += (w * lpx);
            }
        }

        private void FontSizeConvert(ref int lpx, ref int lpy)
        {
            switch (_chrSize)
            {
                case FontSize.Wide:
                    lpx = 2;
                    lpy = 1;
                    break;
                case FontSize.High:
                    lpx = 1;
                    lpy = 2;
                    break;
                case FontSize.WideHigh:
                    lpx = 2;
                    lpy = 2;
                    break;
                case FontSize.WideHighX36:
                    lpx = 6;
                    lpy = 6;
                    break;
                case FontSize.Normal:
                default:
                    lpx = 1;
                    lpy = 1;
                    break;
            }
        }

        public void SetFontSize(FontSize fontSize)
        {
            _chrSize = fontSize;
        }

        public void SetFont(byte[] f)
        {
            _font = f;
            if (f == null)
                _externalfont = false;
            else
                _externalfont = true;
        }

        #endregion
    }
}
