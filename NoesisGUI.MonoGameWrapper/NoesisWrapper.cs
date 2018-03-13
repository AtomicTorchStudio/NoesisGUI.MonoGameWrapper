namespace NoesisGUI.MonoGameWrapper
{
    using System;
    using System.Diagnostics;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Helpers.DeviceState;
    using NoesisGUI.MonoGameWrapper.Input;
    using NoesisGUI.MonoGameWrapper.Providers;
    using SharpDX.Direct3D11;

    using EventArgs = System.EventArgs;

    /// <summary>
    /// Wrapper usage:
    /// 1. at game LoadContent() create wrapper instance
    /// 2. at game Update() invoke wrapper.Update(gameTime)
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
        private readonly NoesisConfig config;

        private readonly Device deviceD3D11;

        private readonly DeviceStateHelper deviceState;

        private readonly GameWindow gameWindow;

        private readonly GraphicsDevice graphicsDevice;

        private readonly NoesisProviderManager providerManager;

        private readonly TimeSpan startupTotalGameTime;

        private InputManager inputManager;

        private bool isEventsSubscribed;

        private bool isPPAAEnabled;

        private View.TessellationQuality quality = View.TessellationQuality.High;

        private View.RenderFlags renderFlags;

        private View view;

        private Renderer viewRenderer;

        static NoesisWrapper()
        {
            // init NoesisGUI (called only once during the game lifetime)
            GUI.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoesisWrapper" /> class.
        /// </summary>
        public NoesisWrapper(NoesisConfig config)
        {
            config.Validate();
            this.config = config;
            this.gameWindow = config.GameWindow;

            // setup Noesis Debug callbacks
            Log.LogCallback = this.NoesisLogCallbackHandler;

            this.graphicsDevice = config.Graphics.GraphicsDevice;
            this.deviceD3D11 = (Device)this.graphicsDevice.Handle;

            this.deviceState = new DeviceStateHelperD3D11((Device)this.graphicsDevice.Handle);

            this.providerManager = config.NoesisProviderManager;
            var provider = this.providerManager.Provider;
            GUI.SetFontProvider(provider.FontProvider);
            GUI.SetTextureProvider(provider.TextureProvider);
            GUI.SetXamlProvider(provider.XamlProvider);

            // setup theme
            if (config.ThemeXamlFilePath != null)
            {
                var themeResourceDictionary = (ResourceDictionary)GUI.LoadXaml(config.ThemeXamlFilePath);
                if (themeResourceDictionary == null)
                {
                    throw new Exception(
                        $"Theme is not found or was not able to load by NoesisGUI: {config.ThemeXamlFilePath}");
                }

                GUI.SetApplicationResources(themeResourceDictionary);
                this.Theme = themeResourceDictionary;
            }

            // create and prepare view
            this.view = this.CreateView(config.RootXamlFilePath);
            this.viewRenderer = this.view.Renderer;
            this.UpdateSize();

            // prepare input
            this.inputManager = new InputManager(
                this.view,
                this.ControlTreeRoot,
                config);

            // subscribe to XNA events
            this.EventsSubscribe();

            // call update with zero delta time to prepare the view for rendering
            this.startupTotalGameTime = config.CurrentTotalGameTime;
            this.Update(new GameTime(this.startupTotalGameTime, elapsedGameTime: TimeSpan.Zero));
        }

        /// <summary>
        /// Gets root element.
        /// </summary>
        public FrameworkElement ControlTreeRoot { get; private set; }

        /// <summary>
        /// Gets the input manager.
        /// </summary>
        public InputManager Input => this.inputManager;

        /// <summary>
        /// Gets or sets the anti-aliasing mode.
        /// </summary>
        public bool IsPPAAEnabled
        {
            get => this.isPPAAEnabled;
            set
            {
                if (this.isPPAAEnabled == value)
                {
                    return;
                }

                this.isPPAAEnabled = value;
                this.ApplyAntiAliasingSetting(this.view);
            }
        }

        /// <summary>
        /// Gets or sets the tesselation quality.
        /// </summary>
        public View.TessellationQuality Quality
        {
            get => this.quality;
            set
            {
                if (this.quality == value)
                {
                    return;
                }

                this.quality = value;
                this.ApplyQualitySetting(this.view);
            }
        }

        /// <summary>
        /// Gets or sets the render flags.
        /// </summary>
        public View.RenderFlags RenderFlags
        {
            get => this.renderFlags;
            set
            {
                if (this.renderFlags == value)
                {
                    return;
                }

                this.renderFlags = value;
                this.ApplyRenderingFlagsSetting(this.view);
            }
        }

        /// <summary>
        /// Gets resource dictionary of theme.
        /// </summary>
        public ResourceDictionary Theme { get; private set; }

        public void Dispose()
        {
            this.Shutdown();
        }

        public void PreRender()
        {
            using (this.deviceState.Remember())
            {
                this.viewRenderer.UpdateRenderTree();
                if (this.viewRenderer.NeedsOffscreen())
                {
                    this.viewRenderer.RenderOffscreen();
                }
            }
        }

        public void Render()
        {
            using (this.deviceState.Remember())
            {
                this.viewRenderer.Render();
            }
        }

        /// <summary>
        /// Updates NoesisGUI.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            gameTime = this.CalculateRelativeGameTime(gameTime);
            this.view.Update(gameTime.TotalGameTime.TotalSeconds);
        }

        /// <summary>
        /// Updates NoesisGUI input.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="isWindowActive">Is game focused?</param>
        public void UpdateInput(GameTime gameTime, bool isWindowActive)
        {
            gameTime = this.CalculateRelativeGameTime(gameTime);
            this.inputManager.Update(gameTime, isWindowActive);
        }

        private void ApplyAntiAliasingSetting(View view)
        {
            var ppaa = this.isPPAAEnabled
                       || this.graphicsDevice.PresentationParameters.MultiSampleCount <= 1;

            view?.SetIsPPAAEnabled(ppaa);
        }

        private void ApplyQualitySetting(View view)
        {
            view?.SetTessellationQuality(this.quality);
        }

        private void ApplyRenderingFlagsSetting(View view)
        {
            if (this.renderFlags != 0)
            {
                view?.SetFlags(this.renderFlags);
            }
        }

        /// <summary>
        /// Calculate game time since time of construction of this wrapper object (startup time).
        /// </summary>
        /// <param name="gameTime">MonoGame game time.</param>
        /// <returns>Time since startup of this wrapper object.</returns>
        private GameTime CalculateRelativeGameTime(GameTime gameTime)
        {
            return new GameTime(gameTime.TotalGameTime - this.startupTotalGameTime, gameTime.ElapsedGameTime);
        }

        private View CreateView(string rootXamlPath)
        {
            var controlTreeRoot = (FrameworkElement)GUI.LoadXaml(rootXamlPath);
            if (controlTreeRoot == null)
            {
                throw new Exception($"UI file \"{rootXamlPath}\" is not found - cannot initialize GUI.");
            }

            using (this.deviceState.Remember())
            {
                this.ControlTreeRoot = controlTreeRoot;
                var view = GUI.CreateView(controlTreeRoot);
                var renderDeviceD3D11 =
                    new RenderDeviceD3D11(this.deviceD3D11.ImmediateContext.NativePointer, sRGB: false);
                view.Renderer.Init(renderDeviceD3D11);
                this.ApplyQualitySetting(view);
                this.ApplyAntiAliasingSetting(view);
                this.ApplyRenderingFlagsSetting(view);
                return view;
            }
        }

        private void DestroyRoot()
        {
            if (this.view == null)
            {
                // already destroyed
                return;
            }

            this.EventsUnsubscribe();

            using (this.deviceState.Remember())
            {
                this.viewRenderer.Shutdown();
                this.viewRenderer = null;
                var viewWeakRef = new WeakReference(this.view);
                this.view = null;
                this.inputManager = null;
                this.ControlTreeRoot = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // ensure the view is GC'ollected
                Debug.Assert(viewWeakRef.Target == null);
            }
        }

        private void DeviceLostHandler(object sender, EventArgs eventArgs)
        {
            // TODO: restore this? not sure where it went in NoesisGUI 2.0
            //Noesis.GUI.DeviceLost();
        }

        private void DeviceResetHandler(object sender, EventArgs e)
        {
            // TODO: restore this? not sure where it went in NoesisGUI 2.0
            //Noesis.GUI.DeviceReset();
            this.UpdateSize();
            this.ApplyAntiAliasingSetting(this.view);
        }

        private void EventsSubscribe()
        {
            if (this.isEventsSubscribed)
            {
                throw new Exception("Events already subscribed");
            }

            this.gameWindow.ClientSizeChanged += this.WindowClientSizeChangedHandler;
            this.graphicsDevice.DeviceReset += this.DeviceResetHandler;
            this.graphicsDevice.DeviceLost += this.DeviceLostHandler;
            this.isEventsSubscribed = true;
        }

        private void EventsUnsubscribe()
        {
            if (!this.isEventsSubscribed)
            {
                return;
            }

            this.gameWindow.ClientSizeChanged -= this.WindowClientSizeChangedHandler;
            this.graphicsDevice.DeviceReset -= this.DeviceResetHandler;
            this.graphicsDevice.DeviceLost -= this.DeviceLostHandler;
            this.isEventsSubscribed = false;
        }

        private void NoesisLogCallbackHandler(LogLevel level, string channel, string message)
        {
            // NoesisGUI 2.1 doesn't have the exception callback anymore
            //this.config.OnExceptionThrown?.Invoke(exception);
            if (level == LogLevel.Error)
            {
                this.config.OnErrorMessageReceived?.Invoke(message);
            }
        }

        private void Shutdown()
        {
            this.DestroyRoot();
            this.Theme = null;
            GUI.UnregisterNativeTypes();
            this.providerManager?.Dispose();
        }

        private void UpdateSize()
        {
            var viewport = this.graphicsDevice.Viewport;
            this.view.SetSize(viewport.Width, viewport.Height);
        }

        private void WindowClientSizeChangedHandler(object sender, EventArgs e)
        {
            this.UpdateSize();
        }
    }
}