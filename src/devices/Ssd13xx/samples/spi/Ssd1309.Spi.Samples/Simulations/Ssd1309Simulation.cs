// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Iot.Device.Ssd13xx;
using Iot.Device.Graphics;

namespace Ssd13xx.Samples.Simulations
{
    /// <summary>
    /// Base class for rendering simulations on SSD1309 displays
    /// </summary>
    public abstract class Ssd1309Simulation
    {
        /// <summary>
        /// Optional value to enable debug logging
        /// </summary>
        protected readonly bool _debug;

        /// <summary>
        /// Instance of the Ssd1309 display to display the simulation
        /// </summary>
        protected readonly Ssd1309 _display;

        /// <summary>
        /// Number of display-ready frames to store in the buffer
        /// </summary>
        protected readonly int _frameBufferSize;

        /// <summary>
        /// Frames-per-second sent to the display
        /// </summary>
        protected readonly int _fps;

        /// <summary>
        /// Frame buffer for display-ready frames
        /// </summary>
        protected ConcurrentQueue<byte[]> _frameBuffer;

        /// <summary>
        /// Stores the current render state
        /// </summary>
        protected BitmapImage _renderState;

        /// <summary>
        /// Semaphore to limit the number of concurent threads that can access rendering resources
        /// </summary>
        protected SemaphoreSlim _bufferSemaphore;

        /// <summary>
        /// Semaphore to limit the number of concurrent threads that can send data to the display
        /// </summary>
        protected SemaphoreSlim _displaySemaphore;

        /// <summary>
        /// Start time of the simulation
        /// </summary>
        protected DateTime _startTime;

        /// <summary>
        /// Max frames of the simulation to run before stopping
        /// </summary>
        protected int _maxFrames = 0;

        /// <summary>
        /// Number of frames sent to the display
        /// </summary>
        protected int _framesSent = 0;

        /// <summary>
        /// Backing field. True if simulation is running
        /// </summary>
        protected bool _isRunning = false;

        /// <summary>
        /// True if simulation is running
        /// </summary>
        public bool IsRunning => IsRunning;

        /// <summary>
        /// Backing field. True if simulation has completed
        /// </summary>
        protected bool _isCompleted = false;

        /// <summary>
        /// True if simulation has completed
        /// </summary>
        public bool IsCompleted => _isCompleted;

        /// <summary>
        /// Base class for rendering simulations on SSD1309 displays
        /// </summary>
        /// <param name="display">Instance of a SSD1309 display</param>
        /// <param name="fps">Frames-per-second to render the simulation</param>
        /// <param name="frameBufferSize">Number of device-ready frames to store in the buffer</param>
        /// <param name="debug">Optional value to enable debug logging</param>
        public Ssd1309Simulation(Ssd1309 display, int fps = 30, int frameBufferSize = 30, bool debug = false)
        {
            _display = display;
            _fps = fps;
            _frameBufferSize = frameBufferSize;
            _debug = debug;
            _frameBuffer = new ConcurrentQueue<byte[]>();
            _bufferSemaphore = new SemaphoreSlim(1);
            _displaySemaphore = new SemaphoreSlim(1);
            _renderState = _display.GetBackBufferCompatibleImage();
        }

        /// <summary>
        /// Starts the simulation
        /// </summary>
        /// <param name="iterations">Number of frames to simulate before completing</param>
        /// <param name="initialDelayMs">Initial delay to buffer frames. If unspecified, delay will be a factor of FPS and buffer size.</param>
        public async virtual Task StartAsync(int iterations, int? initialDelayMs = null)
        {
            _startTime = DateTime.UtcNow;
            _maxFrames += iterations;

            _isRunning = true;

            var displayInterval = 1000 / _fps;

            // Assumes the buffer needs to be topped off when its 50% empty
            var bufferInterval = displayInterval * (_frameBufferSize / 2);

            if (initialDelayMs == null)
            {
                initialDelayMs = bufferInterval * 2;
            }

            // Sends the next frame in the simulation to the device after an initial delay
            var displayTimer = new Timer(async (state) => await SendNextFrame(), null, initialDelayMs.Value, displayInterval);

            // Populates buffer immediatley and upon every bufferInterval thereafter
            var bufferTimer = new Timer(async (state) => await PopulateBufferAsync(), null, 0, bufferInterval);

            Console.WriteLine("Running simulation. Press any key to stop...");

            while (!Console.KeyAvailable)
            {
                if (_framesSent >= iterations)
                {
                    Stop();
                    _isCompleted = true;

                    Console.WriteLine("Simulation completed");
                    break;
                }

                await Task.Delay(bufferInterval);
            }

            if (_isRunning)
            {
                Console.ReadKey();
                Stop();
            }
        }

        /// <summary>
        /// Stops the simulation
        /// </summary>
        public virtual void Stop()
        {
            _isRunning = false;

            Console.WriteLine("Simulation stopped");
        }

        /// <summary>
        /// Timer delegate that dequeues the next available frame from the buffer and sends to the display
        /// </summary>
        protected async virtual Task SendNextFrame()
        {
            if (_isRunning)
            {
                // Ensure only one thread can send data to the display at a time. If busy, skip.
                if (await _displaySemaphore.WaitAsync(0))
                {
                    if (_frameBuffer.TryDequeue(out var nextFrame))
                    {
                        _display.SendData(nextFrame);
                        _framesSent++;
                    }
                    else
                    {
                        if (_debug)
                        {
                            // This indicates that the buffer cannot be populated as fast as the display is attemting to send frames.
                            // Performance improvements could be achieved by optimizing the Update method or the simulation pattern.
                            // One obvious improvement would be to use byte arrays for render state instead of drawing to a bitmap,
                            // but that comes with development tradeoffs for scenarios like drawing text and shapes.
                            Console.WriteLine($"Frame skipped, buffer is empty. Actual FPS: {GetActualFps()}");
                        }
                    }

                    _displaySemaphore.Release();
                }
                else
                {
                    if (_debug)
                    {
                        // This indicates a bottleneck with the device itself. It may be possible to configure the display
                        // for better performance such as increasing clock speed. Otherwise consider reducing FPS.
                        Console.WriteLine($"Frame skipped, previous frame is still being sent. Actual FPS: {GetActualFps()}");
                    }
                }
            }
        }

        /// <summary>
        /// Timer delegate that pre-renders frames and populates the buffer up to the max buffer size
        /// </summary>
        protected async virtual Task PopulateBufferAsync()
        {
            // Ensure only one thread can access rendering resources at a time. If busy, skip.
            if (_isRunning && _renderState != null && await _bufferSemaphore.WaitAsync(0))
            {
                while (_frameBuffer.Count < _frameBufferSize)
                {
                    // Stores to a buffer of display-ready bytes without sending them to the display.
                    // This compensates for frames that may take longer to render than others by rendering on a separate thread and
                    // allows for fewer skipped frames at higher frame rates.
                    Update();
                    _frameBuffer.Enqueue(_display.PreRenderBitmap(_renderState));
                }

                _bufferSemaphore.Release();
            }
        }

        /// <summary>
        /// Updates the render state for the next step of the simulation and returns a copy of the current state
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// Returns the actual FPS expressed as frames sent to the display in the elapsed time
        /// </summary>
        /// <returns></returns>
        protected virtual int GetActualFps()
        {
            var elapsed = DateTime.UtcNow - _startTime;
            return (int)(_framesSent / elapsed.TotalSeconds);
        }
    }
}
