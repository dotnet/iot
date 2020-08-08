// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace Iot.Device.Blinkt.Samples
{
    /// <summary>
    /// One-dimensional Tetris.
    /// </summary>
    public class OneDTetris : IDisposable
    {
        private const int MaxSize = 4;
        private const int MaxGrid = Blinkt.NumPixels + MaxSize - 1;

        private readonly Color _off = Color.Black;

        private readonly Random _random = new Random();

        private readonly List<Color> _grid = Enumerable.Repeat(Color.Black, MaxGrid).ToList();

        private readonly IBlinkt _blinkt = new Blinkt();

        internal void Run()
        {
            PlaceTile(RandomTile(MaxSize));
            Update();

            while (!Console.KeyAvailable)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                if (HasLines())
                {
                    BlinkLines();
                    RemoveLines();
                    PlaceTile(RandomTile(MaxSize));
                }
                else
                {
                    Gravity();
                }

                Update();
            }
        }

        private Color RandomColor() => Color.FromArgb(0, (byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(1, 50));

        private (int, Color) RandomTile(int maxSize, int minSize = 1)
        {
            return (_random.Next(minSize, maxSize), RandomColor());
        }

        private void PlaceTile((int Position, Color Color) tile)
        {
            for (int i = 0; i < tile.Position; i++)
            {
                _grid[MaxGrid - i - 2] = tile.Color;
            }
        }

        private void Update()
        {
            for (int i = 0; i < Blinkt.NumPixels; i++)
            {
                _blinkt.SetPixel(i, _grid[i].R, _grid[i].G, _grid[i].B);
            }

            _blinkt.Show();
        }

        private bool HasLines()
        {
            return _grid[0] != _off;
        }

        private IEnumerable<int> GetLines()
        {
            List<int> lines = new List<int>();

            foreach ((int index, Color color) in _grid.WithIndex())
            {
                if (color == _off)
                {
                    return lines;
                }
                else
                {
                    lines.Add(index);
                }
            }

            return lines;
        }

        private void BlinkLines()
        {
            Hide();
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            Update();
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            Hide();
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            void Hide()
            {
                foreach (int line in GetLines())
                {
                    _blinkt.SetPixel(line, 0, 0, 0);
                }

                _blinkt.Show();
            }
        }

        private void RemoveLines()
        {
            foreach (int line in GetLines())
            {
                _grid[line] = _off;
            }
        }

        private void Gravity()
        {
            _grid.Add(_off);
            _grid.RemoveAt(0);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _blinkt.Dispose();
        }
    }
}
