using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

namespace ArduinoCsCompiler
{
    public static class ErrorManager
    {
        private static List<CompilerMessage> _messages = new List<CompilerMessage>();

        static ErrorManager()
        {
            Logger = null!;
        }

        public static ILogger Logger
        {
            get;
            set; // Set on startup by Run<T>
        }

        public static void Add(CompilerMessage msg)
        {
            Logger.Log(msg.Level, $"{msg.ErrorCode}: {msg.Message}");
            _messages.Add(msg);
        }

        public static void Add(LogLevel level, string errorCode, string message)
        {
            Add(new CompilerMessage(level, errorCode, message));
        }

        public static void AddWarning(string errorCode, string message)
        {
            Add(new CompilerMessage(LogLevel.Warning, errorCode, message));
        }

        public static void AddError(string errorCode, string message)
        {
            Add(new CompilerMessage(LogLevel.Error, errorCode, message));
        }

        public static void PrintImporantMessages()
        {
            Console.WriteLine("Repeating warnings and errors:");
            foreach (var msg in _messages)
            {
                Console.WriteLine(msg.ToString());
            }
        }
    }
}
