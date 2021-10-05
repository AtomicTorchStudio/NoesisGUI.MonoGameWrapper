namespace TestMonoGameNoesisGUI
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using NoesisGUI.MonoGameWrapper;
    using NoesisGUI.MonoGameWrapper.Providers;

    /// <summary>
    /// This is an example MonoGame game using NoesisGUI.
    /// </summary>
    public class GameWithNoesis : Game
    {
        // You can try enabling the fullscreen mode here.
        private const bool SettingIsFullscreen = false;

        // If you want to try the hardware fullscreen mode, you can enable it here.
        // Please note that it has some issues in MonoGame 3.6 and not recommended (was not tested in 3.8).
        private const bool SettingIsFullscreenEnforceHardwareMode = false;

        private readonly GraphicsDeviceManager graphics;

        private TimeSpan lastUpdateTotalGameTime;

        private NoesisWrapper noesisWrapper;

        private SpriteBatch spriteBatch;

        public GameWithNoesis()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.GraphicsProfile = GraphicsProfile.HiDef;
            this.Content.RootDirectory = "Content";
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.noesisWrapper.PreRender();

            this.GraphicsDevice.Clear(
                ClearOptions.Target | ClearOptions.DepthBuffer, // | ClearOptions.Stencil, // we don't use stencil
                Color.Black,
                depth: 1,
                stencil: 0);

            // TODO: Add your drawing code here

            this.noesisWrapper.Render();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;

            this.Window.AllowUserResizing = true;

            if (SettingIsFullscreen || SettingIsFullscreenEnforceHardwareMode)
            {
                // let's setup the fullscreen mode
                if (SettingIsFullscreenEnforceHardwareMode)
                {
                    this.graphics.HardwareModeSwitch = true;
                    this.graphics.PreferredBackBufferWidth = this.GraphicsDevice.DisplayMode.Width;
                    this.graphics.PreferredBackBufferHeight = this.GraphicsDevice.DisplayMode.Height;
                }
                else
                {
                    this.graphics.HardwareModeSwitch = false;
                }

                this.graphics.IsFullScreen = true;
                this.graphics.ApplyChanges();
            }

            this.RefreshBackbufferSize();

            // we need this for NoesisGUI (as we draw it directly into the main framebuffer)
            this.graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
            this.graphics.ApplyChanges();

            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);

            this.CreateNoesisGUI();

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            this.DestroyNoesisGUI();
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            this.lastUpdateTotalGameTime = gameTime.TotalGameTime;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
                return;
            }

            this.RefreshBackbufferSize();

            // update NoesisGUI input only (clicks, key presses, mouse position, etc)
            this.noesisWrapper.UpdateInput(gameTime, isWindowActive: this.IsActive);

            // TODO: Add your game logic update code here

            base.Update(gameTime);

            // update NoesisGUI after updating game logic (it will perform layout and other operations)
            this.noesisWrapper.Update(gameTime);
        }

        private static void NoesisGUIErrorMessageReceivedHandler(string errorMessage)
        {
            if (errorMessage.IndexOf("Binding", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // binding error
                //Global.Logger.Write("NoesisGUI error: " + errorMessage, LogSeverity.Info);
                return;
            }

            if (errorMessage.IndexOf("fallback texture", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // async texture loading
                return;
            }

            Debug.WriteLine("NoesisGUI error: " + errorMessage);
        }

        private static void NoesisGUIUnhandledExceptionHandler(Exception exception)
        {
            Debug.WriteLine("Exception thrown from NoesisGUI context: " + exception);
        }

        private void CreateNoesisGUI()
        {
            // ensure the Noesis.App assembly is loaded otherwise NoesisGUI will be unable to located "Window" type
            System.Reflection.Assembly.Load("Noesis.App");
            
            // TODO: input your license details here
            var licenseName = "My license";
            var licenseKey = "ABCDEFGH";
            NoesisWrapper.Init(licenseName, licenseKey);

            var rootPath = Path.Combine(Environment.CurrentDirectory, "GUI");
            var providerManager = new NoesisProviderManager(
                new FolderXamlProvider(rootPath),
                new FolderFontProvider(rootPath),
                new FolderTextureProvider(rootPath, this.GraphicsDevice));

            var config = new NoesisConfig(
                this.Window,
                this.graphics,
                providerManager,
                rootXamlFilePath: "MainWindow.xaml",
                themeXamlFilePath: "Resources.xaml",
                currentTotalGameTime: this.lastUpdateTotalGameTime,
                callbackGetViewport: this.GetMainComposerViewportForUI,
                onErrorMessageReceived: NoesisGUIErrorMessageReceivedHandler,
                onDevLogMessageReceived: this.NoesisGUIDevLogMessageReceivedHandler,
                onUnhandledException: NoesisGUIUnhandledExceptionHandler);

            config.SetupInputFromWindows();

            this.noesisWrapper = new NoesisWrapper(config);
            this.noesisWrapper.View.IsPPAAEnabled = true;
        }

        private void DestroyNoesisGUI()
        {
            if (this.noesisWrapper == null)
            {
                return;
            }

            this.noesisWrapper.Dispose();
            this.noesisWrapper = null;
        }

        private Viewport GetMainComposerViewportForUI()
        {
            var width = this.graphics.PreferredBackBufferWidth;
            var height = this.graphics.PreferredBackBufferHeight;
            return new Viewport(0, 0, width, height);
        }

        private void NoesisGUIDevLogMessageReceivedHandler(string message)
        {
            if (message.IndexOf("Does not contain a property", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("returned null",            StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // binding error
                return;
            }

            Debug.WriteLine("NoesisGUI DEV log: " + message);
        }

        // There is a bug in MonoGame 3.6 - the game doesn't automatically adjust its backbuffer size to match the game window client bounds.
        // So we have to manually refresh the backbuffer size every frame.
        private void RefreshBackbufferSize()
        {
            if (this.graphics.HardwareModeSwitch
                && this.graphics.IsFullScreen)
            {
                // don't refresh the backbuffer size automatically in fullscreen hardware mode
                return;
            }

            var bounds = this.Window.ClientBounds;
            if (this.graphics.PreferredBackBufferWidth == bounds.Width
                && this.graphics.PreferredBackBufferHeight == bounds.Height)
            {
                return;
            }

            this.graphics.PreferredBackBufferWidth = bounds.Width;
            this.graphics.PreferredBackBufferHeight = bounds.Height;
            this.graphics.ApplyChanges();
        }
    }
}