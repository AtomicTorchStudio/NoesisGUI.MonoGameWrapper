namespace NoesisGUI.MonoGameWrapper
{
    #region

    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Helpers.DeviceState;
    using NoesisGUI.MonoGameWrapper.Input;
    using SharpDX.Direct3D11;

    #endregion

    /// <summary>
    /// Wrapper usage:
    /// 1. at game LoadContent() create wrapper instance
    /// 2. at game Update() invoke wrapper.Update(gameTime)
    /// 3. at game Draw() invoke:
    /// - 3.1. wrapper.PreRender(gameTime)
    /// - 3.2. clear graphics device (including stencil buffer)
    /// - 3.3. your game drawing code
    /// - 3.4. wrapper.PostRender()
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

        private readonly BaseNoesisProviderManager providerManager;

        private readonly TimeSpan startupTotalGameTime;

        private View.AntialiasingMode antiAliasingMode = View.AntialiasingMode.MSAA;

        private InputManager inputManager;

        private bool isEventsSubscribed;

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

            // not implemented in NoesisGUI yet
            // setup Noesis Debug callbacks
            //Debug.ExceptionThrown = this.NoesisExceptionThrownHandler;
            //Debug.ErrorMessageReceived = this.NoesisErrorMessageReceivedHandler;

            this.graphicsDevice = config.Graphics.GraphicsDevice;
            this.FixAntiAliasingMode(ref this.antiAliasingMode);
            this.deviceD3D11 = (Device)this.graphicsDevice.Handle;

            this.deviceState = new DeviceStateHelperD3D11((Device)this.graphicsDevice.Handle);

            // setup resource providerManager
            if (config.NoesisFileSystemProviderRootFolderPath != null)
            {
                GUI.SetResourceProvider(config.NoesisFileSystemProviderRootFolderPath);
            }
            else
            {
                this.providerManager = config.CreateNoesisProviderManagerDelegate();
                GUI.SetResourceProvider(this.providerManager.Provider);
            }

            // setup theme
            if (config.ThemeXamlFilePath != null)
            {
                var themeResourceDictionary = (ResourceDictionary)GUI.LoadXaml(config.ThemeXamlFilePath);
                if (themeResourceDictionary == null)
                {
                    throw new Exception(
                        $"Theme is not found or was not able to load by NoesisGUI: {config.ThemeXamlFilePath}");
                }

                GUI.SetTheme(themeResourceDictionary);
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
            this.Update(new GameTime(this.startupTotalGameTime, elapsedGameTime: TimeSpan.Zero), isWindowActive: true);
        }

        /// <summary>
        /// Gets or sets the anti-aliasing mode.
        /// </summary>
        public View.AntialiasingMode AntiAliasingMode
        {
            get { return this.antiAliasingMode; }
            set
            {
                this.FixAntiAliasingMode(ref value);
                if (this.antiAliasingMode == value)
                {
                    return;
                }

                this.antiAliasingMode = value;
                this.ApplyAntialiasingSetting(this.view);
            }
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
        /// Gets or sets the tesselation quality.
        /// </summary>
        public View.TessellationQuality Quality
        {
            get { return this.quality; }
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
            get { return this.renderFlags; }
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

        public void PostRender()
        {
            using (this.deviceState.Remember())
            {
                this.viewRenderer.Render();
            }
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

        /// <summary>
        /// Updates NoesisGUI.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="isWindowActive">Is game focused?</param>
        public void Update(GameTime gameTime, bool isWindowActive)
        {
            gameTime = new GameTime(gameTime.TotalGameTime - this.startupTotalGameTime, gameTime.ElapsedGameTime);
            this.view.Update(gameTime.TotalGameTime.TotalSeconds);
            this.inputManager.Update(gameTime, isWindowActive);
        }

        private void ApplyAntialiasingSetting(View view)
        {
            view?.SetAntialiasingMode(this.antiAliasingMode);
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
                view.Renderer.InitD3D11(this.deviceD3D11.ImmediateContext.NativePointer, this.config.Options);
                this.ApplyQualitySetting(view);
                this.ApplyAntialiasingSetting(view);
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
                System.Diagnostics.Debug.Assert(viewWeakRef.Target == null);
            }
        }

        private void DeviceLostHandler(object sender, System.EventArgs eventArgs)
        {
            // TODO: restore this? not sure where it went in NoesisGUI 2.0
            //Noesis.GUI.DeviceLost();
        }

        private void DeviceResetHandler(object sender, System.EventArgs e)
        {
            // TODO: restore this? not sure where it went in NoesisGUI 2.0
            //Noesis.GUI.DeviceReset();
            this.UpdateSize();
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

        /// <summary>
        /// Set PPAA anti-aliasing mode if MSAA is not enabled
        /// </summary>
        private void FixAntiAliasingMode(ref View.AntialiasingMode mode)
        {
            if (mode == View.AntialiasingMode.MSAA
                && this.graphicsDevice.PresentationParameters.MultiSampleCount <= 1)
            {
                // MSAA is not enabled - use PPAA in that case
                mode = View.AntialiasingMode.PPAA;
            }
        }

        private void NoesisErrorMessageReceivedHandler(string message)
        {
            this.config.OnErrorMessageReceived?.Invoke(message);
        }

        private void NoesisExceptionThrownHandler(Exception exception)
        {
            this.config.OnExceptionThrown?.Invoke(exception);
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

        private void WindowClientSizeChangedHandler(object sender, System.EventArgs e)
        {
            this.UpdateSize();
        }
    }
}