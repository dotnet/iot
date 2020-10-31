using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

#pragma warning disable CS1591
namespace Arduino.Samples
{
    public class DebugLogStream : Stream
    {
        private readonly Stream _streamImplementation;
        private readonly TextWriter _logStream;
        private int _bytesWrittenToLog;

        public override bool CanRead => _streamImplementation.CanRead;

        public override bool CanSeek => _streamImplementation.CanSeek;

        public override bool CanWrite => _streamImplementation.CanWrite;

        public override long Length => _streamImplementation.Length;

        public override long Position
        {
            get => _streamImplementation.Position;
            set => _streamImplementation.Position = value;
        }

        public DebugLogStream(Stream implementation, TextWriter logStream)
        {
            _streamImplementation = implementation;
            _logStream = logStream;
            _bytesWrittenToLog = 0;
        }

        public override void Flush()
        {
            _streamImplementation.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _streamImplementation.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _streamImplementation.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _streamImplementation.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            foreach (byte b in buffer)
            {
                _logStream.Write(string.Format(CultureInfo.InvariantCulture, "0x{0:X2}, ", b));
                _bytesWrittenToLog++;
                if (_bytesWrittenToLog % 16 == 0)
                {
                    _logStream.WriteLine();
                }
            }

            _streamImplementation.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _streamImplementation.Dispose();
                _logStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
