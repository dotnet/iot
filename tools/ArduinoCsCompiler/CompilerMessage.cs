// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ArduinoCsCompiler
{
    public class CompilerMessage : IEquatable<CompilerMessage>
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

        public bool Equals(CompilerMessage? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Message == other.Message && Level == other.Level && ErrorCode == other.ErrorCode;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is CompilerMessage other && Equals(other));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Message, (int)Level, ErrorCode);
        }

        public static bool operator ==(CompilerMessage? left, CompilerMessage? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CompilerMessage? left, CompilerMessage? right)
        {
            return !Equals(left, right);
        }
    }
}
