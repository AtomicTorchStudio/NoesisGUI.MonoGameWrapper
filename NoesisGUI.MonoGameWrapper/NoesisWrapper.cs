namespace NoesisGUI.MonoGameWrapper
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Input;
    using NoesisGUI.MonoGameWrapper.Providers;
    using Control = System.Windows.Forms.Control;
    using EventArgs = Noesis.EventArgs;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    /// <summary>
    /// Wrapper usage:
    /// 1. at game LoadContent() create wrapper instance
    /// 2. at game Update() invoke:
    /// - 2.1. wrapper.UpdateInput(gameTime)
    /// - 2.2. your game update (game logic)
    /// - 2.3. wrapper.Update(gameTime)
    /// 3. at game Draw() invoke:
    /// - 3.1. wrapper.PreRender(gameTime)
    /// - 3.2. clear graphics device (including stencil buffer)
    /// - 3.3. your game drawing code
    /// - 3.4. wrapper.Render()
    /// 4. at game UnloadContent() call wrapper.Dispose() method.
    /// Please be sure you have IsMouseVisible=true at the MonoGame Game class instance.
    /// </summary>
    public class NoesisWrapper : IDisposable
    {
        private static bool isGuiInitialized;

        private readonly NoesisConfig config;

        private readonly GameWindow gameWindow;

        private readonly GraphicsDevice graphicsDevice;

        private InputManager input;

        //private bool lastIsWindowActive;

        private Size lastSize;

        private Rectangle lastViewportBounds;

        private NoesisProviderManager providerManager;

        private NoesisViewWrapper view;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoesisWrapper" /> class.
        /// </summary>
        public NoesisWrapper(NoesisConfig config)
        {
            config.Validate();
            this.config = config;
            this.gameWindow = config.GameWindow;

            // setup Noesis Debug callbacks
            Log.SetLogCallback(this.NoesisLogCallbackHandler);
            Error.SetUnhandledCallback(this.NoesisUnhandledExceptionHandler);

            GUI.SetSoftwareKeyboardCallback(this.SoftwareKeyboardCallbackHandler);

            this.graphicsDevice = config.Graphics.GraphicsDevice;
            this.providerManager = config.NoesisProviderManager;
            var provider = this.providerManager;
            GUI.SetFontProvider(provider.FontProvider);
            GUI.SetTextureProvider(provider.TextureProvider);
            GUI.SetXamlProvider(provider.XamlProvider);

            // setup theme
            if (config.ThemeXamlFilePath is not null)
            {
                // similar to GUI.LoadApplicationResources(config.ThemeXamlFilePath)
                // but retain the ResourceDictionary to expose it as a Theme property
                // (useful to get application resources)
                var themeResourceDictionary = new ResourceDictionary();
                GUI.SetApplicationResources(themeResourceDictionary);
                GUI.LoadComponent(themeResourceDictionary, config.ThemeXamlFilePath);
                this.Theme = themeResourceDictionary;
            }

            // create and prepare view
            var controlTreeRoot = (FrameworkElement)GUI.LoadXaml(config.RootXamlFilePath);
            this.ControlTreeRoot = controlTreeRoot
                                   ?? throw new Exception(
                                       $"UI file \"{config.RootXamlFilePath}\" is not found - cannot initialize UI");

            this.view = new NoesisViewWrapper(
                controlTreeRoot,
                this.graphicsDevice,
                this.config.CurrentTotalGameTime);
            this.RefreshSize(forceRefresh: true);

            var form = (Form)Control.FromHandle(this.gameWindow.Handle);
            this.input = this.view.CreateInputManager(config, form);

            // subscribe to MonoGame events
            this.EventsSubscribe();
        }

        /// <summary>
        /// Gets root element.
        /// </summary>
        public FrameworkElement ControlTreeRoot { get; private set; }

        /// <summary>
        /// Gets the input manager.
        /// </summary>
        public InputManager Input => this.input;

        /// <summary>
        /// Gets resource dictionary of theme.
        /// </summary>
        public ResourceDictionary Theme { get; private set; }

        public NoesisViewWrapper View => this.view;

        public static void Init(string licenseName, string licenseKey)
        {
            if (isGuiInitialized)
            {
                return;
            }

            // init NoesisGUI (called only once during the game lifetime)
            isGuiInitialized = true;
            GUI.SetLicense(licenseName, licenseKey);
            GUI.Init();
        }

        public void Dispose()
        {
            this.Shutdown();
        }

        public void PreRender()
        {
            this.view.PreRender();
        }

        public void Render()
        {
            this.view.Render();
        }

        /// <summary>
        /// Updates NoesisGUI.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            this.RefreshSize();
            this.view.Update(gameTime);
        }

        /// <summary>
        /// Updates NoesisGUI input.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="isWindowActive">Is game focused?</param>
        public void UpdateInput(GameTime gameTime, bool isWindowActive)
        {
            //if (this.lastIsWindowActive != isWindowActive)
            //{
            //    // workaround for the issue when after switching from a GPU-heavy application NoesisGUI renders incorrectly
            //    this.lastIsWindowActive = isWindowActive;
            //    this.RefreshSize(forceRefresh: true);
            //}

            this.input.Update(gameTime, isWindowActive);
        }

        private void DestroyRoot()
        {
            if (this.view == null)
            {
                // already destroyed
                return;
            }

            this.EventsUnsubscribe();

            this.view.Shutdown();
            this.view = null;
            var viewWeakRef = new WeakReference(this.view);
            this.view = null;
            this.input?.Dispose();
            this.input = null;
            this.ControlTreeRoot = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ensure the view is GC'ollected
            Debug.Assert(viewWeakRef.Target == null);
        }

        private void DeviceLostHandler(object sender, System.EventArgs args)
        {
            // TODO: restore this? not sure where it went in NoesisGUI 2.0
            //Noesis.GUI.DeviceLost();
        }

        private void DeviceResetHandler(object sender, System.EventArgs eventArgs)
        {
            // TODO: restore this? not sure where it went in NoesisGUI 2.0
            //Noesis.GUI.DeviceReset();
            this.RefreshSize(forceRefresh: true);
        }

        private void EventsSubscribe()
        {
            this.graphicsDevice.DeviceReset += this.DeviceResetHandler;
            this.graphicsDevice.DeviceLost += this.DeviceLostHandler;
            this.gameWindow.TextInput += this.GameWindowTextInputHandler;
        }

        private void EventsUnsubscribe()
        {
            this.graphicsDevice.DeviceReset -= this.DeviceResetHandler;
            this.graphicsDevice.DeviceLost -= this.DeviceLostHandler;
            this.gameWindow.TextInput -= this.GameWindowTextInputHandler;
        }

        private void GameWindowTextInputHandler(object sender, TextInputEventArgs e)
        {
            this.input.OnMonoGameChar(e.Character, e.Key);
        }

        private void NoesisLogCallbackHandler(LogLevel level, string channel, string message)
        {
            if (level == LogLevel.Error)
            {
                this.config.OnErrorMessageReceived?.Invoke(message);
            }
            else if (level >= LogLevel.Warning)
            {
                this.config.OnDevLogMessageReceived?.Invoke(message);
            }
        }

        private void NoesisUnhandledExceptionHandler(Exception exception)
        {
            this.config.OnUnhandledException?.Invoke(exception);
        }

        private void RefreshSize(bool forceRefresh = false)
        {
            var viewport = this.config.CallbackGetViewport();
            if (viewport.Bounds != this.lastViewportBounds)
            {
                this.lastViewportBounds = viewport.Bounds;
                forceRefresh = true;
            }

            var size = new Size(viewport.Width, viewport.Height);
            if (!forceRefresh
                && this.lastSize == size)
            {
                return;
            }

            // commented out - this workaround seems to be no longer useful
            /*if (forceRefresh
                && this.lastSize == size)
            {
                // force refresh size
                this.view.SetSize(1601, 901);
            }*/

            this.lastSize = size;
            this.view.SetSize((ushort)viewport.Width, (ushort)viewport.Height);
        }

        private void Shutdown()
        {
            isGuiInitialized = false;

            this.DestroyRoot();
            this.Theme = null;
            this.providerManager.Dispose();
            this.providerManager = null;

            GUI.Shutdown(native: false /* passing 'false' to allow UI reloading */);
        }

        private void SoftwareKeyboardCallbackHandler(UIElement focused, bool open)
        {
            this.input?.SoftwareKeyboardCallbackHandler(focused, open);
        }
    }
}