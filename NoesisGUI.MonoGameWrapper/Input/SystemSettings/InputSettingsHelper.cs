namespace NoesisGUI.MonoGameWrapper.Input.SystemSettings
{
    internal abstract class InputSettingsHelper
    {
        public abstract void GetSystemInputSettings(
            out double keyRepeatDelaySeconds,
            out double keyRepeatIntervalSeconds,
            out double mouseDoubleClickIntervalSeconds);

        internal static WinFormsInputSettingsHelper GetPlatform()
        {
            return WinFormsInputSettingsHelper.Instance;
        }
    }
}