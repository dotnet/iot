using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Iot.Device.Arduino.Sample;

namespace ArduinoCsCompiler
{
    internal class TestRun : Run<TestOptions>
    {
        private ArduinoBoard? _board;

        public TestRun(TestOptions options)
        : base(options)
        {
        }

        public override bool RunCommand()
        {
            if (!ConnectToBoard(CommandLineOptions, out _board))
            {
                return false;
            }

            TestCases.Run(_board);

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            _board?.Dispose();
            _board = null;

            base.Dispose(disposing);
        }
    }
}
