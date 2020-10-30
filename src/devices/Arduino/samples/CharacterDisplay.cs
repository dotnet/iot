using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Globalization;
using System.Text;
using Iot.Device.CharacterLcd;

namespace Iot.Device.Arduino.Sample
{
    internal sealed class CharacterDisplay : IDisposable
    {
        private GpioController _controller;
        private Lcd1602 _display;
        private LcdConsole _textController;

        public CharacterDisplay(ArduinoBoard board)
        {
            _controller = board.CreateGpioController(PinNumberingScheme.Logical);
            _display = new Lcd1602(8, 9, new int[] { 4, 5, 6, 7 }, -1, 1.0f, -1, _controller);
            _display.BlinkingCursorVisible = false;
            _display.UnderlineCursorVisible = false;
            _display.Clear();
            ////for (int i = 0; i < 16; i++)
            ////{
            ////    for (int j = 0; j < 16; j++)
            ////    {
            ////        Span<byte> b = new Span<byte>(new byte[] { (byte)(j + i * 16) });
            ////        _display.Write(b);
            ////    }

            ////    Console.ReadKey();
            ////    _display.Clear();
            ////}

            _textController = new LcdConsole(_display, "SplC780", false);
            _textController.Clear();
            LcdCharacterEncodingFactory f = new LcdCharacterEncodingFactory();
            var cultureEncoding = f.Create(CultureInfo.CurrentCulture, "SplC780", '?', _display.NumberOfCustomCharactersSupported);
            _textController.LoadEncoding(cultureEncoding);
        }

        public LcdConsole Output
        {
            get
            {
                return _textController;
            }
        }

        public void Dispose()
        {
            _display.Dispose();
            _controller.Dispose();
        }
    }
}
