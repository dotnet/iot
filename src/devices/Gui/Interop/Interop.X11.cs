// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SA1307 // 'x' should start with an upper-case letter

namespace Iot.Device.Gui
{
    internal partial class InteropGui
    {
        /// <summary>
        /// Name of the X11 dynamic link library. Install with `sudo apt install libx11-dev` if not present.
        /// </summary>
        internal const string X11 = "libX11.so";

        // Definitions for this file come from X.h and Xlib.h from the Raspberry Pi (32 Bit Raspberry Pi OS)
        // They where tested to work with 64 bit ARM as well.
        internal static UInt32 AllPlanes = 0xFFFF_FFFF;

        internal static UInt32 XYBitmap = 0; /* depth 1, XYFormat */
        internal static UInt32 XYPixmap = 1; /* depth == drawable depth */
        internal static UInt32 ZPixmap = 2; /* depth == drawable depth */

        internal static int KeyPress = 2;
        internal static int KeyReleae = 3;
        internal static int ButtonPress = 4;
        internal static int ButtonRelease = 5;
        internal static int MotionNotify = 6;
        internal static int EnterNotify = 7;
        internal static int LeaveNotify = 8;
        internal static int FocusIn = 9;
        internal static int FocusOut = 10;

        internal static int PointerWindow = 0;

        internal static int NoEventMask = 0;
        internal static int KeyPressMask = (1 << 0);
        internal static int KeyReleaseMask = (1 << 1);
        internal static int ButtonPressMask = (1 << 2);
        internal static int ButtonReleaseMask = (1 << 3);
        internal static int EnterWindowMask = (1 << 4);
        internal static int LeaveWindowMask = (1 << 5);
        internal static int PointerMotionMask = (1 << 6);
        internal static int PointerMotionHintMask = (1 << 7);
        internal static int Button1MotionMask = (1 << 8);
        internal static int Button2MotionMask = (1 << 9);
        internal static int Button3MotionMask = (1 << 10);
        internal static int Button4MotionMask = (1 << 11);
        internal static int Button5MotionMask = (1 << 12);
        internal static int ButtonMotionMask = (1 << 13);
        internal static int KeymapStateMask = (1 << 14);
        internal static int ExposureMask = (1 << 15);
        internal static int VisibilityChangeMask = (1 << 16);
        internal static int StructureNotifyMask = (1 << 17);
        internal static int ResizeRedirectMask = (1 << 18);
        internal static int SubstructureNotifyMask = (1 << 19);
        internal static int SubstructureRedirectMask = (1 << 20);
        internal static int FocusChangeMask = (1 << 21);
        internal static int PropertyChangeMask = (1 << 22);
        internal static int ColormapChangeMask = (1 << 23);
        internal static int OwnerGrabButtonMask = (1 << 24);

        /// <summary>
        /// Opens the display and returns an image pointer
        /// </summary>
        /// <param name="displayName">The display name (can be null)</param>
        /// <returns>The raw image pointer. To get the image data, use <see cref="Marshal.PtrToStructure{XImage}(IntPtr)"/>. The raw pointer is required
        /// in the call to <see cref="XDestroyImage"/></returns>
        [DllImport(X11, CharSet = CharSet.Ansi)]
        private static extern unsafe IntPtr XOpenDisplay(byte* displayName);

        /// <summary>
        /// Opens a named display
        /// </summary>
        /// <param name="displayName">Display name, or null for the default display</param>
        /// <returns>A display handle</returns>
        internal static unsafe IntPtr XOpenDisplay(string displayName)
        {
            var bytes = Encoding.ASCII.GetBytes(displayName);
            try
            {
                fixed (char* p = displayName)
                {
                    return XOpenDisplay((byte*)p);
                }
            }
            catch (DllNotFoundException x)
            {
                throw new DllNotFoundException($"Could not open required library {X11}. Try installing the package libX11-dev.", x);
            }
        }

        /// <summary>
        /// Opens a handle to the primary X11 display
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DllNotFoundException"></exception>
        public static IntPtr XOpenDisplay()
        {
            try
            {
                unsafe
                {
                    return XOpenDisplay((byte*)null);
                }
            }
            catch (DllNotFoundException x)
            {
                // Wrap with additional information.
                throw new DllNotFoundException($"Could not open required library {X11}. Try installing the package libX11-dev.", x);
            }
        }

        /// <summary>
        /// Closes a handle to the X11 display adapter
        /// </summary>
        /// <param name="display">The display handle</param>
        [DllImport(X11)]
        public static extern unsafe void XCloseDisplay(IntPtr display);

        /// <summary>
        /// Gets the X11 root window (usually the desktop)
        /// </summary>
        /// <param name="display">The display to use</param>
        /// <returns>A handle to a window</returns>
        [DllImport(X11)]
        internal static extern unsafe Window XDefaultRootWindow(IntPtr display);

        /// <summary>
        /// Retrieves window attributes
        /// </summary>
        /// <param name="display">The display to operate on</param>
        /// <param name="w">The window handle to use</param>
        /// <param name="window_attributes_return">Out: A window attribute structure</param>
        /// <returns></returns>
        [DllImport(X11)]
        internal static extern int XGetWindowAttributes(
            IntPtr display,
            Window w,
            ref XWindowAttributes window_attributes_return);

        /// <summary>
        /// Returns an user-readable description of a window, for debugging purposes
        /// </summary>
        /// <param name="display">The display handle</param>
        /// <param name="w">The window handle</param>
        /// <returns>A string containing name and size of the given window</returns>
        internal static string GetWindowDescription(IntPtr display, Window w)
        {
            XWindowAttributes attr = default;
            XGetWindowAttributes(display, w, ref attr);

            return $"Window {w}: Location {attr.x}/{attr.y} Size {attr.width}/{attr.height}";
        }

        /// <summary>
        /// Gets the color of a pixel in an image
        /// </summary>
        [DllImport(X11)]
        internal static extern UInt32 XGetPixel(IntPtr image, int x, int y);

        /// <summary>
        /// Free the image.
        /// </summary>
        /// <param name="image">Raw image pointer</param>
        /// <returns>Error code</returns>
        [DllImport(X11)]
        internal static extern int XDestroyImage(IntPtr image);

        /// <summary>
        /// Free an image created by <see cref="XGetImage"/>
        /// </summary>
        /// <param name="image">The image handle to dispose</param>
        /// <returns>Result code</returns>
        [DllImport(X11)]
        internal static extern int XFree(XImage image);

        /// <summary>
        /// Gets an image of the current display/window
        /// </summary>
        /// <param name="display">Display to operate on</param>
        /// <param name="w">Window handle</param>
        /// <param name="x">Left edge of window portion to grab</param>
        /// <param name="y">Top edge of window portion to grab</param>
        /// <param name="width">Width of area to grab</param>
        /// <param name="height">Height of area to grab</param>
        /// <param name="plane_mask">Mask of image planes to get. Normally <see cref="AllPlanes"/></param>
        /// <param name="format">Image format, normally <see cref="ZPixmap"/></param>
        /// <returns></returns>
        [DllImport(X11)]
        internal static extern IntPtr XGetImage(
            IntPtr display,
            Window w, // Window
            int x,
            int y,
            UInt32 width,
            UInt32 height,
            UInt32 plane_mask,
            UInt32 format);

        /// <summary>
        /// Queries the mouse pointer
        /// </summary>
        [DllImport(X11)]
        internal static extern bool XQueryPointer(
            IntPtr display, /* display */
            Window wm, /* w */
            [In, Out] ref Window root_return, /* root_return */
            [In, Out] ref Window child_return, /* child_return */
            [In, Out] ref int root_x_return, /* root_x_return */
            [In, Out] ref int root_y_return, /* root_y_return */
            [In, Out] ref int win_x_return, /* win_x_return */
            [In, Out] ref int win_y_return, /* win_y_return */
            [In, Out] ref uint mask_return); /* mask_return */

        /// <summary>
        /// Send an event to the window message queue
        /// </summary>
        [DllImport(X11)]
        internal static extern int XSendEvent(
            IntPtr display, /* display */
            Window w, /* w */
            bool propagate, /* propagate */
            int event_mask, /* event_mask */
            [In, Out] ref XButtonEvent event_send); /* event_send */

        /// <summary>
        /// Send an event to the window message queue
        /// </summary>
        [DllImport(X11)]
        internal static extern int XSendEvent(
            IntPtr display, /* display */
            Window w, /* w */
            bool propagate, /* propagate */
            int event_mask, /* event_mask */
            [In, Out] ref XMotionEvent event_send); /* event_send */

        /// <summary>
        /// Flush all commands to the display
        /// </summary>
        /// <param name="display">The display handle</param>
        /// <returns>Error return</returns>
        [DllImport(X11)]
        internal static extern int XFlush(IntPtr display);

        /// <summary>
        /// Queries for button press events in the event handler queue
        /// </summary>
        [DllImport(X11)]
        internal static extern int XWindowEvent(
            IntPtr display,
            Window w,
            int event_mask,
            ref XButtonEvent event_return);

        /// <summary>
        /// Queries for mouse move events in the event handler queue
        /// </summary>
        [DllImport(X11)]
        internal static extern int XWindowEvent(
            IntPtr display,
            Window w,
            int event_mask,
            ref XMotionEvent event_return);

        /// <summary>
        /// A method to create a very simplistic window
        /// </summary>
        [DllImport(X11)]
        internal static extern Window XCreateSimpleWindow(
            IntPtr display,
            Window parent,
            int x,
            int y,
            uint width,
            uint height,
            uint border_width,
            Int32 border,
            Int32 background);

        /// <summary>
        /// Enable handling of certain event types by the given window
        /// </summary>
        [DllImport(X11)]
        internal static extern int XSelectInput(
            IntPtr display,
            Window w,
            int event_mask);

        /// <summary>
        /// Make sure the window is visible
        /// </summary>
        [DllImport(X11)]
        internal static extern void XMapWindow(IntPtr display, Window w);

        /// <summary>
        /// Transform mouse positions between windows
        /// </summary>
        [DllImport(X11)]
        internal static extern int XWarpPointer(
            IntPtr display,
            Window src_w,
            Window dest_w,
            int src_x,
            int src_y,
            uint src_width,
            uint src_height,
            int dest_x,
            int dest_y);

        /// <summary>
        /// Look at the next event without actually consuming it
        /// </summary>
        /// <param name="display">The display to use</param>
        /// <param name="event_return">The next event in the queue</param>
        /// <returns>Error return</returns>
        [DllImport(X11)]
        internal static extern int XPeekEvent(IntPtr display, [In, Out] ref XEvent event_return);

        /// <summary>
        /// Hide the given window
        /// </summary>
        [DllImport(X11)]
        internal static extern int XUnmapWindow(
            IntPtr display,
            Window w);

        /// <summary>
        /// Destroy a window and clear its resources
        /// </summary>
        [DllImport(X11)]
        internal static extern int XDestroyWindow(IntPtr display, Window w);

        /// <summary>
        /// Structure with window attributes
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct XWindowAttributes
        {
            public int x; /* location of window */
            public int y; /* location of window */
            public int width; /* width and height of window */
            public int height; /* width and height of window */
            public int border_width; /* border width of window */
            public int depth; /* depth of window */
            public IntPtr visual; /* the associated visual structure */
            public IntPtr root; /* root of screen containing window */
            public int c_class; /* InputOutput, InputOnly*/
            public int bit_gravity; /* one of bit gravity values */
            public int win_gravity; /* one of the window gravity values */
            public int backing_store; /* NotUseful, WhenMapped, Always */
            public uint backing_planes; /* planes to be preserved if possible */
            public uint backing_pixel; /* value to be used when restoring planes */
            public bool save_under; /* boolean, should bits under be saved? */
            public uint colormap; /* color map to be associated with window */
            public bool map_installed; /* boolean, is color map currently installed*/
            public int map_state; /* IsUnmapped, IsUnviewable, IsViewable */
            public int all_event_masks; /* set of events all people have interest in*/
            public int your_event_mask; /* my event mask */
            public int do_not_propagate_mask; /* set of events that should not propagate */
            public bool override_redirect; /* boolean value for override-redirect */
            public IntPtr screen; /* back pointer to correct screen */
        }

        internal unsafe delegate XImage CreateImage(IntPtr display,
            IntPtr visual, UInt32 depth, int format, int offset, IntPtr data, UInt32 width, UInt32 height, int bitmap_pad, int bytes_per_line);

        internal unsafe delegate int DestroyImage(XImage image);

        internal unsafe delegate UInt32 GetPixel(XImage image, int x, int y);

        internal unsafe delegate int PutPixel(XImage image, int x, int y, UInt32 color);

        internal unsafe delegate XImage SubImage(XImage image, int x, int y, UInt32 width, UInt32 height);

        internal unsafe delegate int AddPixel(XImage image, int x, int y, UInt32 color);

        /// <summary>
        /// Structure representing an image in the X11 world
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal class XImage
        {
            public int width; /* size of image */
            public int height; /* size of image */
            public int xoffset; /* number of pixels offset in X direction */
            public int format; /* XYBitmap, XYPixmap, ZPixmap */
            public IntPtr data; /* pointer to image data */
            public int byte_order; /* data byte order, LSBFirst, MSBFirst */
            public int bitmap_unit; /* quant. of scanline 8, 16, 32 */
            public int bitmap_bit_order; /* LSBFirst, MSBFirst */
            public int bitmap_pad; /* 8, 16, 32 either XY or ZPixmap */
            public int depth; /* depth of image */
            public int bytes_per_line; /* accelarator to next line */
            public int bits_per_pixel; /* bits per pixel (ZPixmap) */
            public nuint red_mask; /* bits in z arrangment */
            public nuint green_mask;
            public nuint blue_mask;

            public IntPtr obdata; /* hook for the object routines to hang on */
            /*public ImageFuncs funcs;
            public UInt64 pad1;
            public UInt64 pad2;
            public UInt64 pad3;
            public UInt64 pad4;*/
        }

        /// <summary>
        /// Image function callbacks
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct ImageFuncs
        {
            /* image manipulation routines */
            public CreateImage createImage;
            public DestroyImage destroyImage;
            public GetPixel getPixel;
            public PutPixel putPixel;
            public SubImage subImage;
            public AddPixel addPixel;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Time
        {
            public nint time;
        }

        /// <summary>
        /// A handle to an X11 window
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Window : IEquatable<Window>
        {
            public static readonly Window Zero = new Window();

            /// <summary>
            /// This must be the first and only non-static member of this class!
            /// </summary>
            public nint handle;

            public override bool Equals(object? obj)
            {
                if (obj is Window w)
                {
                    return Equals(w);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return handle.GetHashCode();
            }

            public bool Equals(Window other)
            {
                return handle == other.handle;
            }

            public static bool operator ==(Window a, Window b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Window a, Window b)
            {
                return !a.Equals(b);
            }

            public override string ToString()
            {
                return $"0x{handle:X8}";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XButtonEvent
        {
            public nint type; /* ButtonPress or ButtonRelease */
            public nuint serial; /* # of last request processed by server */
            public nuint send_event; /* true if this came from a SendEvent request */
            public IntPtr display; /* Display the event was read from */
            public Window window; /* ``event'' window it is reported relative to */
            public Window root; /* root window that the event occurred on */
            public Window subwindow; /* child window */
            public Time time; /* milliseconds */
            public int x; /* pointer x, y coordinates in event window */
            public int y; /* pointer x, y coordinates in event window */
            public int x_root; /* coordinates relative to root */
            public int y_root; /* coordinates relative to root */
            public uint state; /* key or button mask */
            public uint button; /* detail */
            public bool same_screen; /* same screen flag */
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XMotionEvent
        {
            public nint type; /* MotionNotify */
            public nuint serial; /* # of last request processed by server */
            public nuint send_event; /* true if this came from a SendEvent request */
            public IntPtr display; /* Display the event was read from */
            public Window window; /* ``event'' window reported relative to */
            public Window root; /* root window that the event occurred on */
            public Window subwindow; /* child window */
            public Time time; /* milliseconds */
            public int x; /* pointer x, y coordinates in event window */
            public int y; /* pointer x, y coordinates in event window */
            public int x_root; /* coordinates relative to root */
            public int y_root; /* coordinates relative to root */
            public uint state; /* key or button mask */
            public uint is_hint; /* detail */
            public uint same_screen; /* same screen flag */
        }

        /// <summary>
        /// Union of the above (add more if you need to have other events, such as keyboard)
        /// </summary>
        /// <remarks>
        /// Note that this is an union. All members start at offset 0, and therefore also the different event types include the type field.
        /// </remarks>
        [StructLayout(LayoutKind.Explicit)]
        internal unsafe struct XEvent
        {
            [FieldOffset(0)]
            public int type;

            [FieldOffset(0)]
            public XButtonEvent xbutton;

            [FieldOffset(0)]
            public XMotionEvent xmotion;

            [FieldOffset(0)]
            public fixed long pad[24];
        }
    }
}
