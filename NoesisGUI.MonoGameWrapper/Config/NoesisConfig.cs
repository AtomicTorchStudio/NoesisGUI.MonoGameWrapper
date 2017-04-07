namespace NoesisGUI.MonoGameWrapper
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Input.SystemSettings;

    public class NoesisConfig
    {
        private bool isInputSet;

        /// <param name="gameWindow">The MonoGame GameWindow instance.</param>
        /// <param name="graphics">Graphics device manager of the game instance.</param>
        /// <param name="rootXamlFilePath">Local XAML file path - will be used as the UI root element.</param>
        /// <param name="themeXamlFilePath">(can be null) Local XAML file path - will be used as global ResourceDictionary (UI style).</param>
        /// <param name="checkIfElementIgnoresHitTest">
        /// Callback to invoke when element is tested for hit test (if callback returns true hit is ignored).
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
            string rootXamlFilePath,
            string themeXamlFilePath,
            TimeSpan currentTotalGameTime,
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
            this.CheckIfElementIgnoresHitTest = checkIfElementIgnoresHitTest;
            this.OnErrorMessageReceived = onErrorMessageReceived;
            this.OnExceptionThrown = onExceptionThrown;
            this.CurrentTotalGameTime = currentTotalGameTime;
        }

        /// <summary>
        /// Noesis <see cref="VGOptions" />. Can be configured only before creating NoesisGUI UI (changes after that are not
        /// applied).
        /// Default setting is good enough, do not change if you don't know what're you doing.
        /// </summary>
        public VGOptions Options { get; set; } = new VGOptions();

        internal HitTestIgnoreDelegate CheckIfElementIgnoresHitTest { get; }

        internal Func<BaseNoesisProviderManager> CreateNoesisProviderManagerDelegate { get; private set; }

        internal TimeSpan CurrentTotalGameTime { get; }

        internal GameWindow GameWindow { get; }

        internal GraphicsDeviceManager Graphics { get; }

        internal double InputKeyRepeatDelaySeconds { get; private set; }

        internal double InputKeyRepeatIntervalSeconds { get; private set; }

        internal double InputMouseDoubleClickIntervalSeconds { get; private set; }

        internal string NoesisFileSystemProviderRootFolderPath { get; private set; }

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

        /// <summary>
        /// Setup provider method - a callback method returning instance of <see cref="BaseNoesisProviderManager" />.
        /// It's called when NoesisGUI is initialized.
        /// </summary>
        /// <param name="createNoesisProviderDelegate"></param>
        public void SetupProviderManager(Func<BaseNoesisProviderManager> createNoesisProviderDelegate)
        {
            this.CreateNoesisProviderManagerDelegate = createNoesisProviderDelegate;
            this.NoesisFileSystemProviderRootFolderPath = null;
        }

        /// <summary>
        /// Setup provider method - simply set the absolute disk folder path to use by NoesisGUI for loading XAML, font and texture
        /// files.
        /// </summary>
        /// <param name="folderPath">Disk folder path (absolute).</param>
        public void SetupProviderSimpleFolder(string folderPath)
        {
            this.NoesisFileSystemProviderRootFolderPath = folderPath;
            this.CreateNoesisProviderManagerDelegate = null;
        }

        internal void Validate()
        {
            if (this.CreateNoesisProviderManagerDelegate == null
                && string.IsNullOrEmpty(this.NoesisFileSystemProviderRootFolderPath))
            {
                throw new Exception(
                    "No NoesisGUI resource provider specified. Please call according method of the "
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
    }
}