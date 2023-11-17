// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace FakeVideoCapture;

internal class Program
{
    private byte[] _blob = Array.Empty<byte>();

    private static int Main(string[] args)
    {
        var p = new Program();
        return p.Start(args);
    }

    private int Start(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Expected a filename as agument");
            return -1;
        }

        var filename = args[0];
        filename = Path.GetFullPath(filename);
        if (!File.Exists(filename))
        {
            Console.WriteLine($"The file {filename} does not exist");
            return -2;
        }

        _blob = File.ReadAllBytes(filename);

        return Sender();
    }

    private int Sender()
    {
        int chunk = 2 * 1024;
        var span = _blob.AsSpan();
        var pause = TimeSpan.FromMilliseconds(50);
        int bufferCount = 0;

        using var stream = Console.OpenStandardOutput();

        int offset = 0;
        int length;
        bool isLastChunk = true;
        while (true)
        {
            if (isLastChunk)
            {
                offset = 0;
                bufferCount = 0;
                isLastChunk = false;
            }

            if (offset + chunk < span.Length)
            {
                length = chunk;
            }
            else
            {
                length = span.Length - offset;
                isLastChunk = true;
            }

            var slice = span.Slice(offset, length);
            stream.Write(slice);
            stream.Flush();
            offset += length;
            bufferCount++;

            if (isLastChunk)
            {
                // Without this, it will loop back from the beginning
                return 0;
            }

            Thread.Sleep(pause);
        }
    }
}
