// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Iot.Device.Common
{
    /// <summary>
    /// A stream that combines a set of files. Can be used to read from a consecutive
    /// list of files, e.g. log files that are split by size or date.
    /// </summary>
    public class FileSetStream : Stream
    {
        private string[] _fileNames;
        private FileStream? _activeFile;
        private int _currentFile;

        /// <summary>
        /// Create a new instance using a set of files.
        /// </summary>
        /// <param name="fileNames">A list of file names</param>
        /// <exception cref="FileNotFoundException">One of the files doesn't exist</exception>
        public FileSetStream(IEnumerable<string> fileNames)
        {
            _fileNames = fileNames.ToArray();
            foreach (var f in fileNames)
            {
                if (!File.Exists(f))
                {
                    throw new FileNotFoundException($"The file {f} does not exist", f);
                }
            }

            _currentFile = -1;
            Loop = false;
        }

        /// <summary>
        /// True to indicate that reading should restart at the beginning when the list ends.
        /// </summary>
        /// <remarks>
        /// If this is true, the stream has no end, and reading will always succeed (unless
        /// the file list is empty)
        /// </remarks>
        public bool Loop
        {
            get;
            set;
        }

        /// <summary>
        /// This returns true.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// This returns false.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// This returns false.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// This is not supported
        /// </summary>
        public override long Length => throw new NotSupportedException("Stream length is unknown");

        /// <summary>
        /// This is not supported
        /// </summary>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <summary>
        /// This does nothing
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Reads data from the stream.
        /// </summary>
        /// <param name="buffer">Buffer to fill</param>
        /// <param name="offset">Offset to start filling the buffer</param>
        /// <param name="count">Maximum number of bytes to read.</param>
        /// <returns>The number of bytes actually read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!EnsureDataAvailable())
            {
                return 0;
            }

            if (_activeFile == null)
            {
                return 0;
            }

            return _activeFile.Read(buffer, offset, count);
        }

        /// <summary>
        /// This operation is unsupported.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This operation is unsupported.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This operation is unsupported.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private bool EnsureDataAvailable()
        {
            if (_activeFile == null || _activeFile.Position >= _activeFile.Length)
            {
                _activeFile?.Dispose();
                _activeFile = null;
                _currentFile = (_currentFile + 1);
                if (_currentFile >= _fileNames.Length)
                {
                    if (Loop)
                    {
                        _currentFile = 0;
                    }
                    else
                    {
                        return false;
                    }
                }

                _activeFile = new FileStream(_fileNames[_currentFile], FileMode.Open, FileAccess.Read);
            }

            return true;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_activeFile != null)
                {
                    _activeFile.Dispose();
                    _activeFile = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
