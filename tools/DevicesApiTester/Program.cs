using DeviceApiTester.Infrastructure;

namespace DeviceApiTester
{
    class Program : CommandLineProgram
    {
        private static int Main(string[] args)
        {
            return new Program().Run(args);
        }
    }
}
