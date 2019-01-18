// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Iot.Device.Mcp23xxx;

namespace Iot.Device.Lcm1602a1
{
    /// <summary>
    /// Supports Lcm1602a1 LCD controller with an optional MCP23008 in case a Gpio header is being used. Ported from: https://github.com/adafruit/Adafruit_Python_CharLCD/blob/master/Adafruit_CharLCD/Adafruit_CharLCD.py
    /// </summary>
    public class Lcm1602a1 : IDisposable
    {
        // When the display powers up, it is configured as follows:
        //
        // 1. Display clear
        // 2. Function set: 
        //    DL = 1; 8-bit interface data 
        //    N = 0; 1-line display 
        //    F = 0; 5x8 dot character font 
        // 3. Display on/off control: 
        //    D = 0; Display off 
        //    C = 0; Cursor off 
        //    B = 0; Blinking off 
        // 4. Entry mode set: 
        //    I/D = 1; Increment by 1 
        //    S = 0; No shift 
        //
        // Note, however, that resetting the device doesn't reset the LCD, so we
        // can't assume that its in that state when a sketch starts (and the
        // LiquidCrystal constructor is called).

        private readonly int _rsPin; // LOW: command.  HIGH: character.
        private readonly int _rwPin; // LOW: write to LCD.  HIGH: read from LCD.
        private readonly int _backlight;
        private readonly int _enablePin; // Activated by a HIGH pulse.
        private readonly int[] _dataPins;
        private readonly bool _usingMcp;

        private GpioController _controller;
        private readonly Mcp23008 _mcpController;

        private DisplayFlags _displayFunction;
        private DisplayFlags _displayControl;
        private DisplayFlags _displayMode;

        private byte _numLines;
        private readonly byte[] _rowOffsets;

        /// <summary>
        /// Initializes a new Lcm1602a1 display object without using a readWrite or backlight pin.
        /// This object will use the board's GPIO pins.
        /// </summary>
        /// <param name="registerSelect">The pin that controls the regsiter select.</param>
        /// <param name="enable">The pin that controls the enable switch.</param>
        /// <param name="data">Collection of pins holding the data that will be printed on the screen.</param>
        public Lcm1602a1(int registerSelect, int enable, int[] data)
            : this(registerSelect, -1, enable, -1, data)
        {
            // Do nothing
        }

        /// <summary>
        /// Initializes a new Lcm1602a1 display object.
        /// This object will use the board's GPIO pins.
        /// </summary>
        /// <param name="registerSelect">The pin that controls the regsiter select.</param>
        /// <param name="readWrite">The pin that controls the read and write switch.</param>
        /// <param name="enable">The pin that controls the enable switch.</param>
        /// <param name="backlight">The pin that controls the backlight of the display.</param>
        /// <param name="data">Collection of pins holding the data that will be printed on the screen.</param>
        public Lcm1602a1(int registerSelect, int readWrite, int enable, int backlight, int[] data)
            : this (null, registerSelect, readWrite, enable, backlight, data)
        {
            // Do Nothing
        }

        /// <summary>
        /// Initializes a new Lcm1602a1 display object.
        /// This object will use the board's GPIO pins, or the McpController pins if one is provided.
        /// </summary>
        /// <param name="mcpController">McpController that is used to provide the pins for the display. Null if we should use board's GPIO pins instead.</param>
        /// <param name="registerSelect">The pin that controls the regsiter select.</param>
        /// <param name="readWrite">The pin that controls the read and write switch.</param>
        /// <param name="enable">The pin that controls the enable switch.</param>
        /// <param name="backlight">The pin that controls the backlight of the display.</param>
        /// <param name="data">Collection of pins holding the data that will be printed on the screen.</param>
        public Lcm1602a1(Mcp23008 mcpController, int registerSelect, int readWrite, int enable, int backlight, int[] data)
        {
            _rwPin = readWrite;
            _rsPin = registerSelect;
            _enablePin = enable;
            _dataPins = data;
            _backlight = backlight;

            _rowOffsets = new byte[4];

            _displayFunction = DisplayFlags.LCD_1LINE | DisplayFlags.LCD_5x8DOTS;

            if (data.Length == 4)
            {
                _displayFunction |= DisplayFlags.LCD_4BITMODE;
            }
            else if (data.Length == 8)
            {
                _displayFunction |= DisplayFlags.LCD_8BITMODE;
            }
            else
            {
                throw new ArgumentException($"The length of the array given to parameter {nameof(data)} must be 4 or 8");
            }

            if (mcpController == null)
            {
                _controller = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) ?
                                    new GpioController(PinNumberingScheme.Logical, new UnixDriver()) :
                                    new GpioController(PinNumberingScheme.Logical, new Windows10Driver());
                _usingMcp = false;
            }
            else
            {
                _mcpController = mcpController;
                _usingMcp = true;
            }

            OpenPin(_rsPin, PinMode.Input);
            if (_rwPin != -1)
            {
                OpenPin(_rwPin, PinMode.Input);
            }
            if (_backlight != -1)
            {
                OpenPin(_backlight, PinMode.Input);
            }
            OpenPin(_enablePin, PinMode.Input);
            foreach (int i in _dataPins)
                OpenPin(i, PinMode.Input);
            // By default, initialize the display with one row and 16 characters.
            Begin(16, 1);
        }

        /// <summary>
        /// Opens a pin and sets the mode.
        /// </summary>
        /// <param name="pinNumber">The pin number to be opened.</param>
        /// <param name="mode">The mode that needs to be set.</param>
        private void OpenPin(int pinNumber, PinMode mode)
        {
            if (_usingMcp)
            {
                _mcpController.SetPinMode(pinNumber, mode);
            }
            else
            {
                _controller.OpenPin(pinNumber, mode);
            }
        }

        /// <summary>
        /// Sets a Pin's mode.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="mode">The mode that needs to be set.</param>
        private void SetPinMode(int pinNumber, PinMode mode)
        {
            if (_usingMcp)
            {
                _mcpController.SetPinMode(pinNumber, mode);
            }
            else
            {
                _controller.SetPinMode(pinNumber, mode);
            }
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="value">The value that needs to be written.</param>
        private void Write(int pinNumber, PinValue value)
        {
            if (_usingMcp)
            {
                _mcpController.WritePin(pinNumber, value);
            }
            else
            {
                _controller.Write(pinNumber, value);
            }
        }

        public void Dispose()
        {
            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }
        }

        /// <summary>
        /// Initializes the display by setting the specified columns and lines.
        /// </summary>
        /// <param name="cols">The columns that will be used by the display.</param>
        /// <param name="lines">The lines that will be used by the display.</param>
        /// <param name="dotSize">The resolution that will be used. 5x8 is the default.</param>
        public void Begin(byte cols, byte lines, DisplayFlags dotSize = DisplayFlags.LCD_5x8DOTS)
        {
            if (lines > 1)
            {
                _displayFunction |= DisplayFlags.LCD_2LINE;
            }

            _numLines = lines;

            SetRowOffsets(0x00, 0x40, 0x14, 0x54);

            // for some 1 line displays you can select a 10 pixel high font
            if ((dotSize != DisplayFlags.LCD_5x8DOTS) && (lines == 1))
            {
                _displayFunction |= DisplayFlags.LCD_5x10DOTS;
            }

            _displayControl = DisplayFlags.LCD_DISPLAYON | DisplayFlags.LCD_CURSOROFF | DisplayFlags.LCD_BLINKOFF;
            _displayMode = DisplayFlags.LCD_ENTRYLEFT | DisplayFlags.LCD_ENTRYSHIFTDECREMENT;

            SetPinMode(_rsPin, PinMode.Output);
            // we can save 1 pin by not using RW. Indicate by passing null instead of a pin
            if (_rwPin != -1)
            {
                SetPinMode(_rwPin, PinMode.Output);
            }
            if (_backlight != -1)
            {
                SetPinMode(_backlight, PinMode.Output);
                Write(_backlight, PinValue.High);
            }
            SetPinMode(_enablePin, PinMode.Output);

            // Do this just once, instead of every time a character is drawn (for speed reasons).
            for (int i = 0; i < _dataPins.Length; ++i)
            {
                SetPinMode(_dataPins[i], PinMode.Output);
            }

            // SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
            // according to datasheet, we need at least 40ms after power rises above 2.7V
            // before sending commands. Arduino can turn on way before 4.5V so we'll wait 50
            DelayMicroseconds(50000);
            
            // Initialize the display.
            Write((byte)0x33);
            Write((byte)0x32);

            Write((byte)((byte)Commands.LCD_DISPLAYCONTROL| (byte)_displayControl));
            Write((byte)((byte)Commands.LCD_FUNCTIONSET | (byte)_displayFunction));
            Write((byte)((byte)Commands.LCD_ENTRYMODESET | (byte)_displayMode));
            Clear();
        }

        private void SetRowOffsets(byte row0, byte row1, byte row2, byte row3)
        {
            _rowOffsets[0] = row0;
            _rowOffsets[1] = row1;
            _rowOffsets[2] = row2;
            _rowOffsets[3] = row3;
        }

        #region High Level Surface Area
        /// <summary>
        /// Clears the LCD.
        /// </summary>
        public void Clear()
        {
            Command((byte)Commands.LCD_CLEARDISPLAY);  // clear display, set cursor position to zero
            DelayMicroseconds(3000);  // this command takes a long time!
        }

        /// <summary>
        /// Moves the cursor to the first line and first column.
        /// </summary>
        public void Home()
        {
            Command((byte)Commands.LCD_RETURNHOME);  // set cursor position to zero
            DelayMicroseconds(3000);  // this command takes a long time!
        }

        /// <summary>
        /// Moves the cursor to an explicit column and row position.
        /// </summary>
        /// <param name="col">The column.</param>
        /// <param name="row">The row.</param>
        public void SetCursor(byte col, byte row)
        {
            if (row >= _rowOffsets.Length)
            {
                row = (byte)(_rowOffsets.Length - 1);    // we count rows starting w/0
            }
            if (row >= _numLines)
            {
                row = (byte)(_numLines - 1);    // we count rows starting w/0
            }

            Command((byte)Commands.LCD_SETDDRAMADDR | (col + _rowOffsets[row]));
        }

        /// <summary>
        /// Disable the display.
        /// </summary>
        public void NoDisplay()
        {
            _displayControl &= ~DisplayFlags.LCD_DISPLAYON;
            Command((byte)Commands.LCD_DISPLAYCONTROL | (byte)_displayControl);
        }

        /// <summary>
        /// Enable the display if previously disabled.
        /// </summary>
        public void Display()
        {
            _displayControl |= DisplayFlags.LCD_DISPLAYON;
            Command((byte)Commands.LCD_DISPLAYCONTROL | (byte)_displayControl);
        }

        /// <summary>
        /// Hide the cursor.
        /// </summary>
        public void NoCursor()
        {
            _displayControl &= ~DisplayFlags.LCD_CURSORON;
            Command((byte)Commands.LCD_DISPLAYCONTROL | (byte)_displayControl);
        }

        /// <summary>
        /// Show the cursor.
        /// </summary>
        public void Cursor()
        {
            _displayControl |= DisplayFlags.LCD_CURSORON;
            Command((byte)Commands.LCD_DISPLAYCONTROL | (byte)_displayControl);
        }

        /// <summary>
        /// Turn off cursor blinking.
        /// </summary>
        public void NoBlink()
        {
            _displayControl &= ~DisplayFlags.LCD_BLINKON;
            Command((byte)Commands.LCD_DISPLAYCONTROL | (byte)_displayControl);
        }

        /// <summary>
        /// Turn on cursor blinking.
        /// </summary>
        public void Blink()
        {
            _displayControl |= DisplayFlags.LCD_BLINKON;
            Command((byte)Commands.LCD_DISPLAYCONTROL | (byte)_displayControl);
        }

        /// <summary>
        /// Move display left one position.
        /// </summary>
        public void ScrollDisplayLeft()
        {
            Command((byte)Commands.LCD_CURSORSHIFT | (byte)DisplayFlags.LCD_DISPLAYMOVE | (byte)DisplayFlags.LCD_MOVELEFT);
        }

        /// <summary>
        /// Move display right one position.
        /// </summary>
        public void ScrollDisplayRight()
        {
            Command((byte)Commands.LCD_CURSORSHIFT | (byte)DisplayFlags.LCD_DISPLAYMOVE | (byte)DisplayFlags.LCD_MOVERIGHT);
        }

        /// <summary>
        /// Set text direction left to right.
        /// </summary>
        public void LeftToRight()
        {
            _displayMode |= DisplayFlags.LCD_ENTRYLEFT;
            Command((byte)Commands.LCD_ENTRYMODESET | (byte)_displayMode);
        }

        /// <summary>
        /// Set text direction right to left.
        /// </summary>
        public void RightToLeft()
        {
            _displayMode &= ~DisplayFlags.LCD_ENTRYLEFT;
            Command((byte)Commands.LCD_ENTRYMODESET | (byte)_displayMode);
        }

        /// <summary>
        /// Right justify text from the cursor.
        /// </summary>
        public void Autoscroll()
        {
            _displayMode |= DisplayFlags.LCD_ENTRYSHIFTINCREMENT;
            Command((byte)Commands.LCD_ENTRYMODESET | (byte)_displayMode);
        }

        /// <summary>
        /// Left justify text from the cursor.
        /// </summary>
        public void NoAutoscroll()
        {
            _displayMode &= ~DisplayFlags.LCD_ENTRYSHIFTINCREMENT;
            Command((byte)Commands.LCD_ENTRYMODESET | (byte)_displayMode);
        }

        /// <summary>
        /// Fill one of the first 8 CGRAM locations with custom characters.
        /// You can find help designing characters at https://www.quinapalus.com/hd44780udg.html.
        /// After creating your character, you can print it by running Print('\x01')
        /// </summary>
        /// <param name="location">Should be between 0 and 7</param>
        /// <param name="charmap">Provide an array of 8 bytes containing the pattern</param>
        public void CreateChar(byte location, params byte[] charmap)
        {
            if (charmap.Length != 8)
            {
                throw new ArgumentException(nameof(charmap));
            }

            location &= 0x7; // we only have 8 locations 0-7
            Command((byte)Commands.LCD_SETCGRAMADDR | (location << 3));

            for (int i = 0; i < 8; i++)
            {
                Write(charmap[i]);
            }
        }

        /// <summary>
        /// Enable the backlight.
        /// </summary>
        public void BacklightOn()
        {
            if (_backlight != -1)
            {
                Write(_backlight, PinValue.High);
            }
        }

        /// <summary>
        /// Disable the backlight.
        /// </summary>
        public void BacklightOff()
        {
            if (_backlight != -1)
            {
                Write(_backlight, PinValue.Low);
            }
        }

        /// <summary>
        /// Write text to display.
        /// </summary>
        /// <param name="value">Text to be displayed. May include new lines.</param>
        public void Print(string value)
        {
            for (int i = 0; i < value.Length; ++i)
            {
                Write(value[i]);
            }
        }
        #endregion // High Level Surface Area

        #region Mid Level Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Command(int value)
        {
            Command((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Command(byte value)
        {
            Send(value, PinValue.Low);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(char value)
        {
            Write((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(byte value)
        {
            Send(value, PinValue.High);
        }
        #endregion // Mid Level Methods

        #region Low Level Methods
        // write either command or data, with automatic 4/8-bit selection
        private void Send(byte value, PinValue mode)
        {
            Write(_rsPin, mode);

            // if there is a RW pin indicated, set it low to Write
            if (_rwPin != -1)
            {
                Write(_rwPin, PinValue.Low);
            }

            if (_displayFunction.HasFlag(DisplayFlags.LCD_8BITMODE))
            {
                Write8bits(value);
            }
            else
            {
                Write4bits((byte)(value >> 4));
                Write4bits(value);
            }
        }

        private void PulseEnable()
        {
            Write(_enablePin, PinValue.Low);
            DelayMicroseconds(1);
            Write(_enablePin, PinValue.High);
            DelayMicroseconds(1);    // enable pulse must be >450ns
            Write(_enablePin, PinValue.Low);
            DelayMicroseconds(100);   // commands need > 37us to settle
        }

        private void Write4bits(byte value)
        {
            for (int i = 0; i < 4; i++)
            {
                DigitalWrite(_dataPins[i], ((value >> i) & 1));
            }

            PulseEnable();
        }

        private void Write8bits(byte value)
        {
            for (int i = 0; i < 8; i++)
            {
                DigitalWrite(_dataPins[i], ((value >> i) & 1));
            }

            PulseEnable();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DigitalWrite(int pin, int value)
        {
            PinValue state = (value == 1) ? PinValue.High : PinValue.Low;
            Write(pin, state);
        }

        private static void DelayMicroseconds(int microseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            long v = (microseconds * System.Diagnostics.Stopwatch.Frequency) / 1000000;
            while (sw.ElapsedTicks < v)
            {
                // Do nothing
            }
        }
        #endregion // Low Level Methods
    }
}
