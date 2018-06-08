namespace NoesisGUI.MonoGameWrapper.Input.VirtualKeys
{
    using System.Windows.Forms;

    public abstract class InputVirtualKeyHelper
    {
        public static InputVirtualKeyHelper GetPlatform()
        {
            return WindowsVirtualKeyHelper.Instance;
        }

        public abstract string KeyCodeToUnicode(Keys key);
    }
}