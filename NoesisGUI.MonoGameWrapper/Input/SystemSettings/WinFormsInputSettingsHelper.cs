namespace NoesisGUI.MonoGameWrapper.Input.SystemSettings
{
    using System.Windows.Forms;

    internal class WinFormsInputSettingsHelper : InputSettingsHelper
    {
        public static readonly WinFormsInputSettingsHelper Instance = new();

        public override void GetSystemInputSettings(
            out double keyRepeatDelaySeconds,
            out double keyRepeatIntervalSeconds,
            out double mouseDoubleClickIntervalSeconds)
        {
            // The keyboard repeat-delay setting, from 0 (approximately 250 millisecond delay)
            // through 3 (approximately 1 second delay).
            var sysKeyboardDelay = SystemInformation.KeyboardDelay;

            // The keyboard repeat-speed setting, from 0 (approximately 2.5 repetitions per second)
            // through 31 (approximately 30 repetitions per second).
            var sysKeyboardRepeat = SystemInformation.KeyboardSpeed;

            // The maximum amount of time, in milliseconds, that can elapse between a first click
            // and a second click for the OS to consider the mouse action a double-click.
            var sysMouseDoubleClickTime = SystemInformation.DoubleClickTime;

            keyRepeatDelaySeconds = Map(sysKeyboardDelay,     0, 3,  0.25,      1);
            keyRepeatIntervalSeconds = Map(sysKeyboardRepeat, 0, 31, 1.0 / 2.5, 1.0 / 30.0);

            mouseDoubleClickIntervalSeconds = sysMouseDoubleClickTime / 1000d;
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private static double Map(
            double inputValue,
            double inputMin,
            double inputMax,
            double outputMin,
            double outputMax)
        {
            inputValue = Clamp(inputValue, inputMin, inputMax);

            var inputPercents = (inputValue - inputMin) / (inputMax - inputMin);
            var outputValue = outputMin + inputPercents * (outputMax - outputMin);

            return outputValue;
        }
    }
}