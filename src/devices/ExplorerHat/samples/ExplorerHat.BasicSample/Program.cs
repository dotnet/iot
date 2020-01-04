using System.Threading;
using Serilog;

namespace Iot.Device.ExplorerHat.BasicSample
{
    internal class Program
    {
        private const int MOTOR_TIME = 2000;

        public static void Main(string[] args)
        {
            // Logging configuration
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            using (var hat = new ExplorerHat())
            {
                hat.Lights.Blue.On();
                hat.Lights.Yellow.On();
                hat.Lights.Green.On();
                hat.Motors.Forwards(0.9);
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Blue.Off();
                hat.Lights.Yellow.Off();
                hat.Lights.Green.Off();
                hat.Motors.Stop();

                hat.Lights.Blue.On();
                hat.Lights.Yellow.On();
                hat.Lights.Red.On();
                hat.Motors.Backwards(0.9);
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Blue.Off();
                hat.Lights.Yellow.Off();
                hat.Lights.Red.Off();
                hat.Motors.Stop();

                hat.Lights.Blue.On();
                hat.Lights.Green.On();
                hat.Motors.One.Speed = 0.8;
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Blue.Off();
                hat.Lights.Green.Off();
                hat.Motors.Stop();

                hat.Lights.Blue.On();
                hat.Lights.Red.On();
                hat.Motors.One.Speed = -0.8;
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Blue.Off();
                hat.Lights.Red.Off();
                hat.Motors.Stop();

                hat.Lights.Yellow.On();
                hat.Lights.Green.On();
                hat.Motors.One.Speed = -0.8;
                hat.Motors.Two.Speed = 0.8;
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Yellow.Off();
                hat.Lights.Green.Off();
                hat.Motors.Stop();

                hat.Lights.Yellow.On();
                hat.Lights.Red.On();
                hat.Motors.One.Speed = 0.8;
                hat.Motors.Two.Speed = -0.8;
                Thread.Sleep(MOTOR_TIME);
                hat.Lights.Yellow.Off();
                hat.Lights.Red.Off();
                hat.Motors.Stop();
            }
        }
    }
}
