namespace NoesisGUI.MonoGameWrapper.Input.VirtualKeys
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    // See http://stackoverflow.com/a/38787314
    internal class WindowsVirtualKeyHelper : InputVirtualKeyHelper
    {
        internal static readonly InputVirtualKeyHelper Instance = new WindowsVirtualKeyHelper();

        private static readonly byte[] KeyboardStateBuffer = new byte[255];

        public override string KeyCodeToUnicode(Keys key)
        {
            Array.Clear(KeyboardStateBuffer, 0, KeyboardStateBuffer.Length);

            var keyboardStateStatus = GetKeyboardState(KeyboardStateBuffer);
            if (!keyboardStateStatus)
            {
                return string.Empty;
            }

            var virtualKeyCode = (uint)key;
            var scanCode = MapVirtualKey(virtualKeyCode, 0);
            var inputLocaleIdentifier = GetKeyboardLayout(0);

            var result = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, KeyboardStateBuffer, result, (int)5, (uint)0, inputLocaleIdentifier);

            return result.ToString();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags,
            IntPtr dwhkl);
    }
}