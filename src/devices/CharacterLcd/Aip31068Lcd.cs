// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device;
using System.Device.I2c;
using System.Drawing;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// 16x2 LCD controller based on the AIP31068, which exposes an HD44780 compatible interface with
    /// an integrated I2C controller.
    /// </summary>
    public class Aip31068Lcd : Hd44780
    {
        private const byte ContrastMask = 0x3F;
        private const byte ContrastSetCommand = 0x70;
        private const byte PowerIconContrastCommand = 0x50;
        private const byte IconDisplayFlag = 0x08;
        private const byte BoosterFlag = 0x04;
        private const byte FollowerControlCommand = 0x6C;
        private const byte InternalOscillatorCommand = 0x14;
        private const byte DefaultContrast = 0x20;

        private byte _contrast;
        private bool _iconDisplayEnabled;
        private bool _boosterEnabled;

        // Static members must appear before non-static members (SA1204)
        private static byte NormalizeContrast(byte value)
        {
            return value > ContrastMask ? ContrastMask : (byte)(value & ContrastMask);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aip31068Lcd"/> class using the specified I2C device.
        /// </summary>
        /// <param name="device">The I2C device used to communicate with the LCD controller.</param>
        /// <param name="contrast">Initial contrast value. Valid range: 0-63.</param>
        /// <param name="iconDisplayEnabled">True to enable the icon display, false to disable it.</param>
        /// <param name="boosterEnabled">True to enable the internal voltage booster, false to disable it.</param>
        public Aip31068Lcd(I2cDevice device, byte contrast = DefaultContrast, bool iconDisplayEnabled = false, bool boosterEnabled = true)
            : base(new Size(16, 2), LcdInterface.CreateI2c(device, true))
        {
            _contrast = NormalizeContrast(contrast);
            _iconDisplayEnabled = iconDisplayEnabled;
            _boosterEnabled = boosterEnabled;

            InitializeController();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Aip31068Lcd"/> class using an existing LCD interface.
        /// </summary>
        /// <param name="lcdInterface">The LCD interface used to communicate with the controller.</param>
        /// <param name="contrast">Initial contrast value. Valid range: 0-63.</param>
        /// <param name="iconDisplayEnabled">True to enable the icon display, false to disable it.</param>
        /// <param name="boosterEnabled">True to enable the internal voltage booster, false to disable it.</param>
        public Aip31068Lcd(LcdInterface lcdInterface, byte contrast = DefaultContrast, bool iconDisplayEnabled = false, bool boosterEnabled = true)
            : base(new Size(16, 2), lcdInterface)
        {
            _contrast = NormalizeContrast(contrast);
            _iconDisplayEnabled = iconDisplayEnabled;
            _boosterEnabled = boosterEnabled;

            InitializeController();
        }

        /// <summary>
        /// Gets or sets the display contrast (0-63).
        /// </summary>
        public byte Contrast
        {
            get => _contrast;
            set
            {
                byte normalized = NormalizeContrast(value);
                if (_contrast == normalized)
                {
                    return;
                }

                _contrast = normalized;
                UpdateContrastConfiguration();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the icon display is enabled.
        /// </summary>
        public bool IconDisplayEnabled
        {
            get => _iconDisplayEnabled;
            set
            {
                if (_iconDisplayEnabled == value)
                {
                    return;
                }

                _iconDisplayEnabled = value;
                UpdateContrastConfiguration();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the voltage booster is enabled.
        /// </summary>
        public bool BoosterEnabled
        {
            get => _boosterEnabled;
            set
            {
                if (_boosterEnabled == value)
                {
                    return;
                }

                _boosterEnabled = value;
                UpdateContrastConfiguration();
            }
        }

        /// <summary>
        /// Performs the extended-instruction initialization sequence required by AIP31068L-compatible controllers.
        /// </summary>
        /// <remarks>
        /// Sequence (extended mode): enter extended instruction set; program bias/oscillator; set contrast low nibble
        /// (0x70 | C[3:0]) and high bits with Ion/Bon (0x50 | Ion | Bon | C[5:4]); enable follower (Fon, Rab[2:0]);
        /// wait for VLCD to stabilize; return to basic instruction set.
        /// References (AIP31068L Product Specification):
        /// - Table 3. Instruction Table (p.18/28)
        /// - Section 4.5 "INITIALIZING BY INSTRUCTION" (p.20/28)
        /// - Section 5.2 "BIAS VOLTAGE DIVIDE CIRCUIT" (p.23/28)
        /// Datasheet: https://www.orientdisplay.com/wp-content/uploads/2022/08/AIP31068L.pdf
        /// </remarks>
        private void InitializeController()
        {
            EnterExtendedInstructionSet();
            SendCommandAndWait(InternalOscillatorCommand);
            SendContrastCommands();
            SendCommandAndWait(FollowerControlCommand);
            DelayHelper.DelayMilliseconds(200, allowThreadYield: true);
            ExitExtendedInstructionSet();

            // Re-issue the standard configuration commands expected after extended setup.
            SendCommandAndWait((byte)_displayControl);
            Clear();
            SendCommandAndWait((byte)_displayMode);
        }

        private void UpdateContrastConfiguration()
        {
            EnterExtendedInstructionSet();
            SendContrastCommands();
            ExitExtendedInstructionSet();
        }

        /// <summary>
        /// Writes the electronic volume (contrast) low and high bits while in the extended instruction set.
        /// </summary>
        /// <remarks>
        /// Issues 0x70 | C[3:0] (low nibble) then 0x50 | Ion | Bon | C[5:4] (high bits and power/icon controls).
        /// References (AIP31068L Product Specification): Table 3. Instruction Table (p.18/28) and
        /// Section 4.5 "INITIALIZING BY INSTRUCTION" (p.20/28).
        /// Datasheet: https://www.orientdisplay.com/wp-content/uploads/2022/08/AIP31068L.pdf
        /// </remarks>
        private void SendContrastCommands()
        {
            SendCommandAndWait((byte)(ContrastSetCommand | (_contrast & 0x0F)));
            byte value = (byte)(PowerIconContrastCommand
                | (_iconDisplayEnabled ? IconDisplayFlag : 0)
                | (_boosterEnabled ? BoosterFlag : 0)
                | ((_contrast >> 4) & 0x03));
            SendCommandAndWait(value);
        }

        private void EnterExtendedInstructionSet()
        {
            SendCommandAndWait((byte)(_displayFunction | DisplayFunction.ExtendedInstructionSet));
        }

        private void ExitExtendedInstructionSet()
        {
            SendCommandAndWait((byte)(_displayFunction & ~DisplayFunction.ExtendedInstructionSet));
        }
    }
}

