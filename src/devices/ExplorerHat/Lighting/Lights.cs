using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;

namespace Iot.Device.ExplorerHat.Lighting
{
    /// <summary>
    /// Represents the Explorer HAT led array
    /// </summary>
    public class Lights : IDisposable
    {
        private const int LED1_PIN = 4;
        private const int LED2_PIN = 17;
        private const int LED3_PIN = 27;
        private const int LED4_PIN = 5;

        private GpioController _controller;

        private List<Led> LedArray { get; set; }

        /// <summary>
        /// Blue led (#1)
        /// </summary>
        public Led One { get => LedArray[0]; }

        /// <summary>
        /// Yellow led (#2)
        /// </summary>
        public Led Two { get => LedArray[1]; }

        /// <summary>
        /// Red led (#3)
        /// </summary>
        public Led Three { get => LedArray[2]; }

        /// <summary>
        /// Green led (#4)
        /// </summary>
        public Led Four { get => LedArray[3]; }

        /// <summary>
        /// Blue led (#1)
        /// </summary>
        public Led Blue { get => LedArray[0]; }

        /// <summary>
        /// Yellow led (#2)
        /// </summary>
        public Led Yellow { get => LedArray[1]; }

        /// <summary>
        /// Red led (#3)
        /// </summary>
        public Led Red { get => LedArray[2]; }

        /// <summary>
        /// Green led (#4)
        /// </summary>
        public Led Green { get => LedArray[3]; }

        /// <summary>
        /// Gets the <see cref="Led"/> at the specified index
        /// </summary>
        /// <param name="key">The zero-based (0 to 3) of the led to get</param>
        /// <returns>The <see cref="Led"/> at the specified index</returns>
        public Led this[int key]
        {
            get
            {
                if (key < 0 || key > 3)
                {
                    throw new Exception("Leds are 0..3");
                }

                return LedArray[key];
            }
        }

        /// <summary>
        /// Gets the <see cref="Led"/> at the specified index
        /// </summary>
        /// <param name="key">The color-string-index (blue, yellow, red or green) of the led to get</param>
        /// <returns>The <see cref="Led"/> at the specified index</returns>
        public Led this[string key]
        {
            get
            {
                var strKey = key.ToLower();
                if (strKey != "blue" && strKey != "yellow" && strKey != "red" && strKey != "green")
                {
                    throw new Exception("Leds are blue, yellow, red or green");
                }

                var result = LedArray.Where(l => l.Name.ToLower() == strKey).First();

                return result;
            }
        }

        /// <summary>
        /// Initializes a <see cref="Lights"/> instance
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="Lights"/> to manage GPIO resources</param>
        internal Lights(GpioController controller)
        {
            _controller = controller;

            LedArray = new List<Led>()
            {
                new Led(1, "blue", LED1_PIN, _controller),
                new Led(2, "yellow", LED2_PIN, _controller),
                new Led(3, "red", LED3_PIN, _controller),
                new Led(4, "green", LED4_PIN, _controller)
            };
        }

        /// <summary>
        /// Switch on all led lights
        /// </summary>
        public void On()
        {
            LedArray[0].On();
            LedArray[1].On();
            LedArray[2].On();
            LedArray[3].On();
        }

        /// <summary>
        /// Switch off all led lights
        /// </summary>
        public void Off()
        {
            LedArray[0].Off();
            LedArray[1].Off();
            LedArray[2].Off();
            LedArray[3].Off();
        }

        #region IDisposable Support

        private bool _disposedValue = false; // Para detectar llamadas redundantes

        /// <summary>
        /// Disposes the <see cref="Lights"/> instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    LedArray[0].Dispose();
                    LedArray[1].Dispose();
                    LedArray[2].Dispose();
                    LedArray[3].Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="Lights"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
