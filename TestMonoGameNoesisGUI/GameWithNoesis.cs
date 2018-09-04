namespace TestMonoGameNoesisGUI
{
    #region

    using System;
    using System.IO;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using NoesisGUI.MonoGameWrapper;
    using NoesisGUI.MonoGameWrapper.Providers;

    #endregion

    /// <summary>
    /// This is an example MonoGame game using NoesisGUI.
    /// </summary>
    public class GameWithNoesis : Game
    {
        #region Constants

        // You can try enabling the fullscreen mode here.
        private const bool SettingIsFullscreen = true;

        // If you want to try the hardware fullscreen mode, you can enable it here.
        // Please note that it has some issues in MonoGame 3.6 and not recommended.
        private const bool SettingIsFullscreenEnforceHardwareMode = false;

        #endregion

        #region Fields

        readonly GraphicsDeviceManager graphics;

        private TimeSpan lastUpdateTotalGameTime;

        private NoesisWrapper noesisWrapper;

        SpriteBatch spriteBatch;

        #endregion

        #region Constructors and Destructors

        public GameWithNoesis()
        {
            graphics = new GraphicsDeviceManager(this);
            this.graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
        }

        #endregion

        #region Methods

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.noesisWrapper.PreRender();

            this.GraphicsDevice.Clear(
                ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
                Color.CornflowerBlue,
                1,
                0);

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

            RefreshBackbufferSize();

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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            CreateNoesisGUI();

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            DestroyNoesisGUI();
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
                Exit();
                return;
            }

            RefreshBackbufferSize();

            // update NoesisGUI input only (clicks, key presses, mouse position, etc)
            this.noesisWrapper.UpdateInput(gameTime, isWindowActive: this.IsActive);

            // TODO: Add your game logic update code here

            base.Update(gameTime);

            // update NoesisGUI after updating game logic (it will perform layout and other operations)
            this.noesisWrapper.Update(gameTime);
        }

        private void CreateNoesisGUI()
        {
            var rootPath = Path.Combine(Environment.CurrentDirectory, "Data");
            var providerManager = new NoesisProviderManager(
                new FolderXamlProvider(rootPath),
                new FolderFontProvider(rootPath),
                new FolderTextureProvider(rootPath, this.GraphicsDevice));

            var config = new NoesisConfig(
                this.Window,
                this.graphics,
                providerManager,
                rootXamlFilePath: "Samples/TextBox.xaml",
                themeXamlFilePath: null,
                // uncomment this line to use theme file
                //themeXamlFilePath: "Themes/WindowsStyle.xaml",
                currentTotalGameTime: this.lastUpdateTotalGameTime);

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

        #endregion
    }
}