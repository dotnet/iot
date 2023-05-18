// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Interop;

#pragma warning disable CS1591
namespace Iot.Device.Graphics
{
    /// <summary>
    /// Creates and maintains a low-level window on X11 based systems. This class is experimental.
    /// </summary>
    public class LowLevelX11Window : IDisposable
    {
        private IntPtr _display;
        private Window _rootWindow;
        private Thread? _messageThread;
        private bool _terminateThread;

        public LowLevelX11Window()
        {
            _rootWindow = Window.Zero;
            _display = XOpenDisplay();
            _terminateThread = false;
            if (_display == IntPtr.Zero)
            {
                throw new NotSupportedException("Unable to open display");
            }
        }

        public event Action<object, Point, MouseButton>? MouseStateChanged;

        ~LowLevelX11Window()
        {
            Dispose(false);
        }

        private MouseButton GetButtonEvent(Window w)
        {
            XButtonEvent ev = new XButtonEvent();
            XWindowEvent(_display, w, -1, ref ev);
            if (ev.button != 0)
            {
                Console.WriteLine("Button was pressed");
                return MouseButton.Left;
            }

            return MouseButton.None;
        }

        private Window CreateWindowInternal(int x, int y, int width, int height)
        {
            return XCreateSimpleWindow(_display, XDefaultRootWindow(_display), 0, 0, 100, 200, 2, 0x0FEEDDCC, 0x0FAA0020);
        }

        public void CreateWindow(int x, int y, int width, int height)
        {
            _rootWindow = CreateWindowInternal(x, y, width, height);
        }

        public void StartMessageLoop()
        {
            _terminateThread = false;
            _messageThread = new Thread(MessageLoop);
            _messageThread.Name = "Message loop";
            _messageThread.Start();
        }

        private void MessageLoop()
        {
            if (_rootWindow == Window.Zero)
            {
                throw new InvalidOperationException("Window not created");
            }

            XMapWindow(_display, _rootWindow);
            XSelectInput(_display, _rootWindow, ButtonPressMask | ButtonReleaseMask | PointerMotionMask | ButtonMotionMask);
            while (!_terminateThread)
            {
                XEvent ev1 = default;
                XPeekEvent(_display, ref ev1);
                if (ev1.type == ButtonPress || ev1.type == ButtonRelease)
                {
                    XButtonEvent ev2 = default;
                    XWindowEvent(_display, _rootWindow, ButtonPressMask | ButtonReleaseMask, ref ev2);
                    Console.WriteLine($"Mouse event at {ev2.x}, {ev2.y}: New mask {ev2.button} state {ev2.state}");
                    MouseStateChanged?.Invoke(this, new Point(ev2.x, ev2.y), MouseButton.None); // Todo: Map mouse buttons
                }
                else if (ev1.type == MotionNotify)
                {
                    XMotionEvent ev2 = default;
                    XWindowEvent(_display, _rootWindow, ButtonMotionMask | PointerMotionMask, ref ev2);
                    Console.WriteLine($"Mouse motion notify at {ev2.x}, {ev2.y}: State {ev2.state}");
                }
                else
                {
                    Console.WriteLine($"An event of type {ev1.type} is ignored");
                }
            }
        }

        public void CloseWindow()
        {
            if (_rootWindow != Window.Zero)
            {
                _terminateThread = true;
                XUnmapWindow(_display, _rootWindow);
                XDestroyWindow(_display, _rootWindow);
                _rootWindow = Window.Zero;
            }
        }

        private void ReleaseUnmanagedResources()
        {
            CloseWindow();
            XCloseDisplay(_display);
            _display = IntPtr.Zero;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _terminateThread = true;
                if (_messageThread != null)
                {
                    _messageThread.Join();
                }
            }

            ReleaseUnmanagedResources();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
