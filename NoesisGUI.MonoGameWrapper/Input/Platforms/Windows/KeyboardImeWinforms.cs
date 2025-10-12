namespace NoesisGUI.MonoGameWrapper.Input.Platforms.Windows
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using Noesis;
    using Control = System.Windows.Forms.Control;
    using EventArgs = System.EventArgs;
    using Marshal = System.Runtime.InteropServices.Marshal;
    using Point = System.Drawing.Point;
    using Size = System.Drawing.Size;
    using TextBox = Noesis.TextBox;
    using View = Noesis.View;

    // Partially based on Win32Native.cs by Xenko (MIT License) https://github.com/xenko3d/xenko
    // but has essential changes to make it work for Korean.
    internal class KeyboardImeWinforms : IDisposable
    {
        public static TextBox focusedNoesisTextBox;

        private static string currentCompositionString;

        private static bool isImeComposingNow;

        private static string lastCompositionString = string.Empty;

        private readonly Action<string> errorCallback;

        private readonly Control form;

        private readonly IntPtr parentWndProc;

        // hack that uses a text box to receive IME text input
        private readonly RichTextBoxEx winformsTextBox;

        private readonly IntPtr winformsTextBoxWndProc;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Win32Native.WndProc wndProcDelegate;

        private string finishedComposedString;

        private int imeComposingNowTextStartIndex;

        private bool isDisposed;

        private bool isImeInputEnabled;

        public KeyboardImeWinforms(Control form, Action<string> errorCallback)
        {
            this.form = form;
            this.errorCallback = errorCallback;

            this.winformsTextBox = new RichTextBoxEx()
            {
                Location = new Point(-100, -100),
                Size = new Size(0, 0)
            };

            this.winformsTextBox.TextChanged += this.WinformsTextBoxOnTextChanged;

            // Assign custom window procedure to this text box
            this.parentWndProc = Win32Native.GetWindowLong(form.Handle,
                                                           Win32Native.WindowLongType.WndProc);
            // This is needed to prevent garbage collection of the delegate.
            this.wndProcDelegate = this.WndProc;
            var inputWndProcPtr = Marshal.GetFunctionPointerForDelegate(this.wndProcDelegate);
            this.winformsTextBoxWndProc = Win32Native.SetWindowLong(this.winformsTextBox.Handle,
                                                                    Win32Native.WindowLongType.WndProc,
                                                                    inputWndProcPtr);
        }

        public bool IsImeComposingNow => isImeComposingNow
                                         || this.finishedComposedString is not null;

        public void DisableInput()
        {
            if (!this.isImeInputEnabled)
            {
                return;
            }

            focusedNoesisTextBox = null;

            this.isImeInputEnabled = false;
            isImeComposingNow = false;
            lastCompositionString = currentCompositionString = null;

            if (!this.isDisposed)
            {
                this.form.Focus();
                this.form.Controls.Remove(this.winformsTextBox);
                this.form.Focus();
                this.UpdateCursor();
            }

            Debug.WriteLine("IME: input disabled");
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.DisableInput();

            if (this.winformsTextBox == null)
            {
                return;
            }

            var action = new Action(() =>
                                    {
                                        this.winformsTextBox.TextChanged -= this.WinformsTextBoxOnTextChanged;
                                        this.winformsTextBox.Dispose();
                                    });

            if (this.winformsTextBox.InvokeRequired)
            {
                this.winformsTextBox.Invoke(action);
            }
            else
            {
                action();
            }
        }

        public void EnableInput(UIElement focused)
        {
            // IME input disabled due to incompatibility with Steam Deck
            // (causes screen flash and then unrecoverable stuttering)
            return;

            if (this.isImeInputEnabled)
            {
                return;
            }

            this.isImeInputEnabled = true;
            focusedNoesisTextBox = (TextBox)focused;
            this.form.Controls.Add(this.winformsTextBox);
            this.winformsTextBox.Focus();
            lastCompositionString = currentCompositionString = null;
            this.Clean();
            this.UpdateCursor();
            Debug.WriteLine("IME: input enabled");
        }

        public string TryGetFinishedComposedString()
        {
            var result = this.finishedComposedString;
            if (result is not null)
            {
                this.finishedComposedString = null;
                // it's important to reset this as well so the textbox will be updated
                lastCompositionString = null;
                Debug.WriteLine("IME: last composition string is <null> now");
            }

            return result;
        }

        public void Update(View view)
        {
            if (!isImeComposingNow
                || focusedNoesisTextBox == null
                || focusedNoesisTextBox.View != view)
            {
                return;
            }

            // it's important to call it here again
            this.UpdateCursor();

            if (this.finishedComposedString is not null)
            {
                return;
            }

            if (lastCompositionString == currentCompositionString)
            {
                return;
            }

            lastCompositionString = currentCompositionString;
            focusedNoesisTextBox.SelectedText = lastCompositionString;
            Debug.WriteLine("IME: focused textbox selected text set to: " + lastCompositionString);
        }

        private static unsafe string GetCompositionString(IntPtr context, int type)
        {
            var length = Win32Native.ImmGetCompositionString(context, type, IntPtr.Zero, 0);
            if (length == 0)
            {
                return string.Empty;
            }

            var data = stackalloc byte[length];
            Win32Native.ImmGetCompositionString(context, type, new IntPtr(data), length);
            return Encoding.Unicode.GetString(data, length);
        }

        private void Clean()
        {
            this.winformsTextBox.Text = string.Empty;
            this.imeComposingNowTextStartIndex = 0;
            currentCompositionString = lastCompositionString = null;
            Debug.WriteLine("IME: clean");

            this.UpdateCursor();
        }

        private void CommitTextFromWinformsTextbox()
        {
            string text;
            if (isImeComposingNow)
            {
                text = this.winformsTextBox.Text;
                Debug.WriteLine("IME: (composing) winformsTextBox.Text: " + text);
            }
            else
            {
                text = this.winformsTextBox.GetTextBase();
                Debug.WriteLine("IME: (NOT composing) winformsTextBox.Text: " + text);
            }

            if (text.Length == 0)
            {
                return;
            }

            // Filter out characters that do not belong in text input
            var inputString = text;

            try
            {
                if (isImeComposingNow)
                {
                    var imeComposingNowTextEndIndex = text.Length;
                    inputString = inputString.Substring(this.imeComposingNowTextStartIndex,
                                                        imeComposingNowTextEndIndex
                                                        - this.imeComposingNowTextStartIndex);
                    this.imeComposingNowTextStartIndex = imeComposingNowTextEndIndex;
                }
                else
                {
                    inputString = inputString.Substring(this.imeComposingNowTextStartIndex);
                }
            }
            catch (Exception ex)
            {
                this.errorCallback?.Invoke("Exception during CommitTextFromWinformsTextbox: " + ex.Message);
                return;
            }

            inputString = inputString.Replace("\r", string.Empty).Replace("\n", string.Empty)
                                     .Replace("\t", string.Empty);

            if (inputString.Length > 0)
            {
                if (this.finishedComposedString == null)
                {
                    this.finishedComposedString = inputString;
                }
                else
                {
                    this.finishedComposedString += inputString;
                }
            }

            if (!isImeComposingNow)
            {
                this.Clean();
            }
        }

        private void OnComposition(IntPtr hWnd, int lParam)
        {
            if (lParam == 0)
            {
                return;
            }

            Debug.WriteLine("IME: composing: lParam=0x" + lParam.ToString("X"));
            if ((lParam & Win32Native.GCS_COMPSTR) == 0)
            {
                return;
            }

            // Update the composition string
            var context = Win32Native.ImmGetContext(hWnd);
            currentCompositionString = GetCompositionString(context, Win32Native.GCS_COMPSTR);
            Debug.WriteLine("IME: current composition string: " + currentCompositionString);
            Win32Native.ImmReleaseContext(hWnd, context);

            this.UpdateCursor();
        }

        private void UpdateCursor()
        {
            if (!this.isImeInputEnabled
                || !isImeComposingNow)
            {
                this.winformsTextBox.Location = new Point(-100, -100);
                //Debug.WriteLine("IME: composition overlay location reset");
                return;
            }

            var caretIndex = (uint)Math.Max(focusedNoesisTextBox.CaretIndex, 0);
            var rect = focusedNoesisTextBox.GetRangeBounds(caretIndex, caretIndex);

            var matrix = focusedNoesisTextBox.TextView.TransformToAncestor(focusedNoesisTextBox.View.Content);
            var vector = new Vector4(rect.Location.X,
                                     (float)(rect.Location.Y + rect.Size.Height * 1.1),
                                     0,
                                     1)
                         * matrix;

            var location = new Point((int)vector.X, (int)vector.Y + 4);
            var oldLocation = this.winformsTextBox.Location;
            if (oldLocation != location)
            {
                Debug.WriteLine(
                    $"IME composition screen location: {location.X};{location.Y} - old location {oldLocation.X};{oldLocation.Y}");

                this.winformsTextBox.Location = location;
            }

            // adjust IME window location otherwise it might stuck in the bottom right corner
            // in case of multi-part IME composition (when a remainder is available)
            var hwnd = this.winformsTextBox.Handle;
            var context = Win32Native.ImmGetContext(hwnd);
            var compositionForm = new Win32Native.COMPOSITIONFORM
            {
                dwStyle = Win32Native.CFS_POINT
            };

            Win32Native.ImmSetCompositionWindow(context, ref compositionForm);
            Win32Native.ImmReleaseContext(hwnd, context);
        }

        private void WinformsTextBoxOnTextChanged(object sender, EventArgs args)
        {
            this.CommitTextFromWinformsTextbox();
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch ((Win32MessageCode)msg)
            {
                case (Win32MessageCode)0x118: // Cursor blink message
                    return new IntPtr(0);

                case Win32MessageCode.WM_KEYDOWN:
                case Win32MessageCode.WM_SYSKEYDOWN:
                case Win32MessageCode.WM_KEYUP:
                case Win32MessageCode.WM_SYSKEYUP:
                case Win32MessageCode.WM_CHAR:
                    return Win32Native.CallWindowProc(this.parentWndProc, hWnd, msg, wParam, lParam);

                case Win32MessageCode.WM_IME_STARTCOMPOSITION:
                    Debug.WriteLine("IME: start composition");
                    isImeComposingNow = true;
                    this.Clean();
                    break;

                case Win32MessageCode.WM_IME_ENDCOMPOSITION:
                    Debug.WriteLine("IME: end composition");
                    isImeComposingNow = false;
                    break;

                case Win32MessageCode.WM_IME_COMPOSITION:
                    this.OnComposition(hWnd, (int)lParam);
                    break;

                case Win32MessageCode.WM_NCPAINT:
                case Win32MessageCode.WM_PAINT:
                    var paintStruct = new Win32Native.PAINTSTRUCT();
                    Win32Native.BeginPaint(hWnd, ref paintStruct);
                    Win32Native.EndPaint(hWnd, ref paintStruct);
                    return new IntPtr(0); // Don't paint the control
            }

            return Win32Native.CallWindowProc(this.winformsTextBoxWndProc, hWnd, msg, wParam, lParam);
        }

        public class RichTextBoxEx : RichTextBox
        {
            private const int EM_GETTEXTEX = (WM_USER + 94);

            private const int EM_GETTEXTLENGTHEX = (WM_USER + 95);

            // Flags for the GETEXTEX data structure  
            private const int GT_DEFAULT = 0;

            private const int GTL_CLOSE = 4; // Fast computation of a "close" answer 

            // Flags for the GETTEXTLENGTHEX data structure 
            private const int GTL_DEFAULT = 0; // Do default (return # of chars) 

            private const int WM_USER = 0x0400;

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public override string Text
            {
                get
                {
                    var getLength = new GETTEXTLENGTHEX();
                    getLength.flags = GTL_CLOSE; //get buffer size

                    getLength.codepage = 1200; //Unicode

                    var textLength = SendMessage(this.Handle,
                                                 EM_GETTEXTLENGTHEX,
                                                 ref getLength,
                                                 0);
                    var getText = new GETTEXTEX();
                    getText.cb = textLength + 2; //add space for null terminator

                    getText.flags = GT_DEFAULT;
                    getText.codepage = 1200; //Unicode

                    var sb = new StringBuilder(getText.cb);

                    SendMessage(this.Handle, EM_GETTEXTEX, ref getText, sb);
                    return sb.ToString();
                }
                set => base.Text = value;
            }

            public override int TextLength
            {
                get
                {
                    var getLength = new GETTEXTLENGTHEX();
                    getLength.flags = GTL_DEFAULT; //Returns the number of characters
                    getLength.codepage = 1200;     //Unicode

                    return SendMessage(this.Handle,
                                       EM_GETTEXTLENGTHEX,
                                       ref getLength,
                                       0);
                }
            }

            public string GetTextBase()
            {
                return base.Text;
            }

            [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
            private static extern int SendMessage(
                IntPtr hWnd,
                int msg,
                ref GETTEXTEX wParam,
                StringBuilder lParam);

            [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
            private static extern int SendMessage(
                IntPtr hWnd,
                int msg,
                ref GETTEXTLENGTHEX wParam,
                int lParam);

            [StructLayout(LayoutKind.Sequential)]
            private struct GETTEXTEX
            {
                public int cb;

                public int flags;

                public int codepage;

                public IntPtr lpDefaultChar;

                public IntPtr lpUsedDefChar;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct GETTEXTLENGTHEX
            {
                public int flags;

                public int codepage;
            }
        }
    }
}