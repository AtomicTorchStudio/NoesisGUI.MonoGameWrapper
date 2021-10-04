namespace NoesisGUI.MonoGameWrapper.Input.Platforms.Windows
{
    using System;
    using System.Runtime.InteropServices;

    // based on Win32Native.cs by Xenko (MIT License) https://github.com/xenko3d/xenko
    internal static partial class Win32Native
    {
        public const uint CFS_FORCE_POSITION = 0x0020;

        public const uint CFS_POINT = 0x0002;

        public const int GCS_COMPSTR = 0x0008;

        public const int GCS_RESULTCLAUSE = 0x1000;

        public const int GCS_RESULTSTR = 0x0800;

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public enum WindowLongType : int
        {
            WndProc = (-4),

            HInstance = (-6),

            HwndParent = (-8),

            Style = (-16),

            ExtendedStyle = (-20),

            UserData = (-21),

            Id = (-12),
        }

        [DllImport("user32.dll", EntryPoint = "BeginPaint")]
        public static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT paintStruct);

        [DllImport("user32.dll", EntryPoint = "CallWindowProc", CharSet = CharSet.Unicode)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("ole32.dll")]
        public static extern int CoInitialize(IntPtr pvReserved);

        [DllImport("user32.dll", EntryPoint = "DispatchMessage", CharSet = CharSet.Unicode)]
        public static extern int DispatchMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll", EntryPoint = "EndPaint")]
        public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT paintStruct);

        [DllImport("user32.dll", EntryPoint = "GetFocus", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern sbyte GetMessage(
            out NativeMessage lpMsg,
            IntPtr hWnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax);

        [DllImport("user32.dll", EntryPoint = "GetMessage")]
        public static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public static IntPtr GetWindowLong(IntPtr hWnd, WindowLongType index)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, index);
            }

            return GetWindowLong64(hWnd, index);
        }

        [DllImport("imm32.dll", EntryPoint = "ImmGetCompositionString", CharSet = CharSet.Unicode)]
        public static extern int ImmGetCompositionString(IntPtr himc, int dwIndex, IntPtr buf, int bufLen);

        [DllImport("imm32.dll", EntryPoint = "ImmGetContext")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll", EntryPoint = "ImmReleaseContext")]
        public static extern IntPtr ImmReleaseContext(IntPtr hWnd, IntPtr context);

        [DllImport("imm32.dll")]
        public static extern int ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompositionForm);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool PeekMessage(
            out NativeMessage lpMsg,
            IntPtr hWnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax,
            uint wRemoveMsg);

        [DllImport("user32.dll", EntryPoint = "PeekMessage")]
        public static extern int PeekMessage(
            out NativeMessage lpMsg,
            IntPtr hWnd,
            int wMsgFilterMin,
            int wMsgFilterMax,
            int wRemoveMsg);

        [DllImport("user32.dll", EntryPoint = "SetParent", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        public static IntPtr SetWindowLong(IntPtr hwnd, WindowLongType index, IntPtr wndProcPtr)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLong32(hwnd, index, wndProcPtr);
            }

            return SetWindowLongPtr64(hwnd, index, wndProcPtr);
        }

        public static bool ShowWindow(IntPtr hWnd, bool windowVisible)
        {
            return ShowWindow(hWnd, windowVisible ? 1 : 0);
        }

        [DllImport("user32.dll", EntryPoint = "TranslateMessage", CharSet = CharSet.Unicode)]
        public static extern int TranslateMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLong32(IntPtr hwnd, WindowLongType index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLong64(IntPtr hwnd, WindowLongType index);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLong32(IntPtr hwnd, WindowLongType index, IntPtr wndProc);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, WindowLongType index, IntPtr wndProc);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Unicode)]
        private static extern bool ShowWindow(IntPtr hWnd, int mCmdShow);

        [StructLayout(LayoutKind.Sequential)]
        public struct C_POINT
        {
            public int x;

            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct C_RECT
        {
            public int _Left;

            public int _Top;

            public int _Right;

            public int _Bottom;
        }

        public struct COMPOSITIONFORM
        {
            public uint dwStyle;

            public C_POINT ptCurrentPos;

            public C_RECT rcArea;
        }

        /// <summary>
        /// Internal class to interact with Native Message
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeMessage
        {
            public IntPtr handle;

            public uint msg;

            public IntPtr wParam;

            public IntPtr lParam;

            public uint time;

            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PAINTSTRUCT
        {
            public IntPtr Hdc;

            public bool Erase;

            public RECT PaintRectangle;

            public bool Restore;

            public bool IncUpdate;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;

            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left, Top, Right, Bottom;
        }
    }
}