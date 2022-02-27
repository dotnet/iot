using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ArduinoCsCompiler
{
    public class CompilerMessage
    {
        public CompilerMessage(LogLevel level, string errorCode, string message)
        {
            Message = message;
            Level = level;
            ErrorCode = errorCode;
        }

        public string Message { get; }

        public LogLevel Level { get; }
        public string ErrorCode { get; }

        public override string ToString()
        {
            string level = Level switch
            {
                LogLevel.Critical => "[CRI]",
                LogLevel.Information => "[INF]",
                LogLevel.Debug => "[DEB]",
                LogLevel.Error => "[ERR]",
                LogLevel.Trace => "[TRA]",
                LogLevel.Warning => "[WRN]",
                _ => "[???]",
            };

            return $"{level} {ErrorCode}: {Message}";
        }
    }
}
