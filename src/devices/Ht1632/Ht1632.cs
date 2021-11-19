// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.Ht1632
{
    /// <summary>
    /// HT1632C
    /// 32x8 and 24x16 LED Driver
    /// </summary>
    public class Ht1632 : IDisposable
    {
        // Datasheet: https://www.holtek.com/documents/10179/116711/HT1632D_32D-2v100.pdf

        /// <summary>
        /// Set if system oscillator is on. (default is off)
        /// </summary>
        public bool Enabled { set => WriteCommand(value ? Command.SystemEnabled : Command.SystemDisabled); }

        /// <summary>
        /// Set if LED duty cycle generator is on (default is off)
        /// </summary>
        public bool LedOn { set => WriteCommand(value ? Command.LedOn : Command.LedOff); }

        /// <summary>
        /// Set if blinking function is on (default is off)
        /// </summary>
        public bool Blink { set => WriteCommand(value ? Command.BlinkOn : Command.BlinkOff); }

        /// <summary>
        /// Set clock mode (default is RC-Primary)
        /// </summary>
        public ClockMode ClockMode { set => WriteCommand((Command)value); }

        /// <summary>
        /// Set COM option (default is N-MOS open drain output and 8 COM)
        /// </summary>
        public ComOption ComOption { set => WriteCommand((Command)value); }

        /// <summary>
        /// Set row PWM duty (1/16 to 16/16, default is 16)
        /// </summary>
        public byte PwmDuty { set => WriteCommand(s_pwmCommandMap[value - 1]); }

        private static readonly Command[] s_pwmCommandMap = new[]
        {
            Command.PwmDuty1,
            Command.PwmDuty2,
            Command.PwmDuty3,
            Command.PwmDuty4,
            Command.PwmDuty5,
            Command.PwmDuty6,
            Command.PwmDuty7,
            Command.PwmDuty8,
            Command.PwmDuty9,
            Command.PwmDuty10,
            Command.PwmDuty11,
            Command.PwmDuty12,
            Command.PwmDuty13,
            Command.PwmDuty14,
            Command.PwmDuty15,
            Command.PwmDuty16,
        };

        private readonly Ht1632PinMapping _pinMapping;
        private readonly int _cs;
        private readonly int _wr;
        private readonly int _data;
        private readonly bool _shouldDispose;

        private GpioController? _controller;

        /// <summary>
        /// HT1632C 32x8 and 24x16 LED Driver
        /// </summary>
        /// <param name="pinMapping">The pin mapping to use by the binding.</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling.</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public Ht1632(Ht1632PinMapping pinMapping, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || gpioController is null;
            _controller = gpioController ?? new GpioController();
            _pinMapping = pinMapping;
            _cs = _pinMapping.ChipSelect;
            _wr = _pinMapping.WriteClock;
            _data = _pinMapping.SerialData;

            SetupPins();
        }

        /// <summary>
        /// Command Mode
        /// </summary>
        /// <param name="commands">Commands to sent</param>
        public void WriteCommand(params Command[] commands)
        {
            Begin();

            WriteBits((byte)Id.COMMAND, 3);

            foreach (Command command in commands)
            {
                WriteBits((byte)(command), 8);
                WriteBits(0, 1);
            }

            End();
        }

        /// <summary>
        /// WRITE Mode - Successive Address Writing
        /// </summary>
        /// <param name="address">Memory Address (MA) - 0b00_A6_A5_A4_A3_A2_A1_A0</param>
        /// <param name="data">Data (MA, MA+1, ...) - 0b0000_D0_D1_D2_D3</param>
        public void WriteData(byte address, params byte[] data)
        {
            Begin();

            WriteBits((byte)Id.WRITE, 3);
            WriteBits(address, 7);

            foreach (byte d in data)
            {
                WriteBits(d, 4);
            }

            End();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
        }

        private void SetupPins()
        {
            if (_cs >= 0 && _wr >= 0 && _data >= 0)
            {
                _controller?.OpenPin(_cs, PinMode.Output);
                _controller?.OpenPin(_wr, PinMode.Output);
                _controller?.OpenPin(_data, PinMode.Output);

                _controller?.Write(_cs, 1);
                _controller?.Write(_wr, 1);
                _controller?.Write(_data, 1);
            }
            else
            {
                throw new Exception($"{nameof(Ht1632)} -- {nameof(Ht1632PinMapping)} values must be non-zero; Values: {nameof(Ht1632PinMapping.ChipSelect)}: {_cs}; {nameof(Ht1632PinMapping.WriteClock)}: {_wr}; {nameof(Ht1632PinMapping.SerialData)}: {_data};.");
            }
        }

        private void Begin()
        {
            _controller?.Write(_cs, 0);
        }

        private void End()
        {
            _controller?.Write(_cs, 1);
        }

        private void WriteBits(byte bits, int count = 8)
        {
            for (var i = count - 1; i >= 0; i--)
            {
                var bit = bits >> i & 1;

                _controller?.Write(_wr, 0);
                _controller?.Write(_data, bit);
                _controller?.Write(_wr, 1);
            }
        }
    }
}
