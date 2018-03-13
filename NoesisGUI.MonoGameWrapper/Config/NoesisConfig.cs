namespace NoesisGUI.MonoGameWrapper
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Input.SystemSettings;
    using NoesisGUI.MonoGameWrapper.Providers;

    public class NoesisConfig
    {
        private bool isInputSet;

        /// <param name="gameWindow">The MonoGame GameWindow instance.</param>
        /// <param name="graphics">Graphics device manager of the game instance.</param>
        /// <param name="noesisProviderManager">NoesisGUI Provider Manager (create it before calling this method).</param>
        /// <param name="rootXamlFilePath">Local XAML file path - will be used as the UI root element.</param>
        /// <param name="themeXamlFilePath">
        /// (can be null) Local XAML file path - will be used as global ResourceDictionary (UI
        /// style).
        /// </param>
        /// <param name="checkIfElementIgnoresHitTest">
        /// Callback to invoke when element is tested for hit test (if callback returns true hit is ignored).
        /// It has default implementation so you can skip this.
        /// </param>
        /// <param name="onErrorMessageReceived">Callback to invoke when error message received from NoesisGUI.</param>
        /// <param name="onExceptionThrown">
        /// Callback to invoke when exception thrown from NoesisGUI context (can be in event
        /// handler, etc).
        /// </param>
        /// <param name="currentTotalGameTime">Current game time (needed to do proper Update() calls).</param>
        public NoesisConfig(
            GameWindow gameWindow,
            GraphicsDeviceManager graphics,
            NoesisProviderManager noesisProviderManager,
            TimeSpan currentTotalGameTime,
            string rootXamlFilePath,
            string themeXamlFilePath = null,
            HitTestIgnoreDelegate checkIfElementIgnoresHitTest = null,
            Action<string> onErrorMessageReceived = null,
            Action<Exception> onExceptionThrown = null)
        {
            if (string.IsNullOrEmpty(rootXamlFilePath))
            {
                throw new ArgumentNullException(
                    nameof(rootXamlFilePath),
                    "File path to the root xaml element cannot be null");
            }

            //if (string.IsNullOrEmpty(themeXamlFilePath))
            //{
            //    throw new ArgumentNullException(
            //        nameof(themeXamlFilePath),
            //        "File path to the theme xaml element cannot be null");
            //}

            if (gameWindow == null)
            {
                throw new ArgumentNullException(nameof(gameWindow));
            }

            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            this.GameWindow = gameWindow;
            this.Graphics = graphics;
            this.RootXamlFilePath = rootXamlFilePath.Replace('/', '\\');
            this.ThemeXamlFilePath = themeXamlFilePath?.Replace('/', '\\');
            this.CheckIfElementIgnoresHitTest =
                checkIfElementIgnoresHitTest ?? this.DefaultCheckIfElementIgnoresHitTest;
            this.OnErrorMessageReceived = onErrorMessageReceived;
            this.OnExceptionThrown = onExceptionThrown;
            this.CurrentTotalGameTime = currentTotalGameTime;
            this.NoesisProviderManager = noesisProviderManager;
        }

        internal HitTestIgnoreDelegate CheckIfElementIgnoresHitTest { get; }

        internal TimeSpan CurrentTotalGameTime { get; }

        internal GameWindow GameWindow { get; }

        internal GraphicsDeviceManager Graphics { get; }

        internal double InputKeyRepeatDelaySeconds { get; private set; }

        internal double InputKeyRepeatIntervalSeconds { get; private set; }

        internal double InputMouseDoubleClickIntervalSeconds { get; private set; }

        internal NoesisProviderManager NoesisProviderManager { get; }

        internal Action<string> OnErrorMessageReceived { get; }

        internal Action<Exception> OnExceptionThrown { get; }

        internal string RootXamlFilePath { get; }

        internal string ThemeXamlFilePath { get; }

        public void SetupInput(
            double keyRepeatDelaySeconds = 0.2,
            double keyRepeatIntervalSeconds = 0.05,
            double mouseDoubleClickIntervalSeconds = 0.25)
        {
            this.InputKeyRepeatDelaySeconds = keyRepeatDelaySeconds;
            this.InputKeyRepeatIntervalSeconds = keyRepeatIntervalSeconds;
            this.InputMouseDoubleClickIntervalSeconds = mouseDoubleClickIntervalSeconds;
            this.isInputSet = true;
        }

        public void SetupInputFromWindows()
        {
            double keyRepeatDelaySeconds,
                   keyRepeatIntervalSeconds,
                   mouseDoubleClickIntervalSeconds;

            InputSettingsHelper.GetPlatform()
                               .GetSystemInputSettings(
                                   out keyRepeatDelaySeconds,
                                   out keyRepeatIntervalSeconds,
                                   out mouseDoubleClickIntervalSeconds);

            this.SetupInput(
                keyRepeatDelaySeconds,
                keyRepeatIntervalSeconds,
                mouseDoubleClickIntervalSeconds);
        }

        internal void Validate()
        {
            if (this.NoesisProviderManager == null)
            {
                throw new Exception(
                    "No NoesisGUI Provider specified. Please call according method of the "
                    + nameof(NoesisConfig));
            }

            if (!this.isInputSet)
            {
                throw new Exception(
                    "The input is not setup. Please call according method of the " + nameof(NoesisConfig));
            }

            if (this.Graphics.GraphicsProfile != GraphicsProfile.HiDef)
            {
                throw new Exception("NoesisGUI requires GraphicsProfile.HiDef set to MonoGame Graphics Device");
            }
        }

        private bool DefaultCheckIfElementIgnoresHitTest(Visual visual)
        {
            // focusable elements do not ignore hit tests
            return !(visual as FrameworkElement)?.Focusable ?? false;
        }
    }
}