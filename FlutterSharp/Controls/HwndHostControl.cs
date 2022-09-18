using System;
using FlutterSharp.Utilities;

namespace FlutterSharp.Controls
{
    public sealed class HwndHostControl : System.Windows.Forms.Control
    {
        private readonly IntPtr _hwnd;

        public IntPtr ChildWnd
        {
            get { return _hwnd; }
        }

        public HwndHostControl(IntPtr childWnd, bool insertToWindow)
        {
            CreateControl();
            _hwnd = childWnd;
            if (insertToWindow) InsertToWindow();
        }

        public HwndHostControl(IntPtr childWnd) : this(childWnd, true)
        {
        }

        public void InsertToWindow()
        {
            NativeMethods.SetParent(_hwnd, Handle);
            NativeMethods.MoveWindow(_hwnd, 0, 0, Width, Height, true);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            NativeMethods.SetFocus(_hwnd);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            NativeMethods.MoveWindow(_hwnd, 0, 0, Width, Height, true);
        }
    }
}