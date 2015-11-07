namespace TestMonoGameNoesisGUI
{
	#region

	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;
	using Microsoft.Xna.Framework.Input;

	using NoesisGUI.MonoGameWrapper;

	#endregion

	/// <summary>
	///     This is an example MonoGame game using NoesisGUI
	/// </summary>
	public class GameWithNoesis : Game
	{
		#region Fields

		GraphicsDeviceManager graphics;

		private MonoGameNoesisGUIWrapper noesisGUIWrapper;

		SpriteBatch spriteBatch;

		#endregion

		#region Constructors and Destructors

		public GameWithNoesis()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		#endregion

		#region Methods

		/// <summary>
		///     This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			this.noesisGUIWrapper.PreRender(gameTime);

			this.GraphicsDevice.Clear(
				ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
				Color.CornflowerBlue,
				1,
				0);

			// TODO: Add your drawing code here

			this.noesisGUIWrapper.PostRender();

			base.Draw(gameTime);
		}

		/// <summary>
		///     Allows the game to perform any initialization it needs to before starting to run.
		///     This is where it can query for any required services and load any non-graphic
		///     related content.  Calling base.Initialize will enumerate through any components
		///     and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			this.IsMouseVisible = true;

			this.noesisGUIWrapper = new MonoGameNoesisGUIWrapper(
				this,
				this.graphics,
				"TextBox.xaml",
				stylePath: "NoesisStyle.xaml",
				dataLocalPath: "Data");

			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		///     LoadContent will be called once per game and is the place to load
		///     all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		///     UnloadContent will be called once per game and is the place to unload
		///     game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		///     Allows the game to run logic such as updating the world,
		///     checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
			    || Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				Exit();
			}

			this.noesisGUIWrapper.Update(gameTime);

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		#endregion
	}
}