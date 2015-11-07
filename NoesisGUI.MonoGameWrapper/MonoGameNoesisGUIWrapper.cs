namespace NoesisGUI.MonoGameWrapper
{
	#region

	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

	using Noesis;

	using NoesisGUI.MonoGameWrapper.Helpers;
	using NoesisGUI.MonoGameWrapper.Input;

	using SharpDX.Direct3D11;

	using EventArgs = System.EventArgs;

	#endregion

	/// <summary>
	///     Wrapper usage:
	///     1. at game Initialize() create wrapper instance (see this class constructor)
	///     2. at game Update() invoke wrapper.Update(gameTime)
	///     3. at game Draw() invoke:
	///     - 3.1. wrapper.PreRender(gameTime)
	///     - 3.2. clear graphics device (including stencil buffer)
	///     - 3.3. your game drawing code
	///     - 3.4. wrapper.PostRender()
	///     Please be sure you have IsMouseVisible=true at the MonoGame instance
	/// </summary>
	public class MonoGameNoesisGUIWrapper
	{
		#region Fields

		private readonly Device DeviceDX11;

		private readonly DeviceDX11StateHelper deviceState = new DeviceDX11StateHelper();

		private readonly Game game;

		private readonly GraphicsDeviceManager graphics;

		private readonly GraphicsDevice graphicsDevice;

		private readonly MonoGameNoesisGUIWrapperInputManager inputManager;

		private readonly UIRenderer uiRenderer;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		///     Initializes a new instance of the <see cref="MonoGameNoesisGUIWrapper" /> class.
		/// </summary>
		/// <param name="game">The MonoGame game instance.</param>
		/// <param name="graphics">Graphics device manager of the game instance.</param>
		/// <param name="rootXamlPath">Local XAML file path - will be used as the UI root element</param>
		/// <param name="stylePath">(optional) Local XAML file path - will be used as global ResourceDictionary (UI style)</param>
		/// <param name="dataLocalPath">(optional) Local path to the folder which will be used as root for other paths</param>
		/// <remarks>
		///     PLEASE NOTE: .XAML-files should be prebuilt to .NSB-files by NoesisGUI Build Tool).
		/// </remarks>
		public MonoGameNoesisGUIWrapper(
			Game game,
			GraphicsDeviceManager graphics,
			string rootXamlPath,
			string stylePath = null,
			string dataLocalPath = "Data")
		{
			this.game = game;
			this.graphics = graphics;

			this.graphicsDevice = graphics.GraphicsDevice;
			var device = ((Device)this.graphicsDevice.Handle);
			this.DeviceDX11 = device;

			GUI.InitDirectX11(device.NativePointer);

			GUI.AddResourceProvider(dataLocalPath);

			this.uiRenderer = this.CreateRenderer(rootXamlPath, stylePath);

			this.inputManager = new MonoGameNoesisGUIWrapperInputManager(this.uiRenderer);
			game.Window.TextInput += (sender, args) => this.inputManager.OnTextInput(args);
			game.Window.ClientSizeChanged += this.WindowClientSizeChangedHandler;
			this.graphicsDevice.DeviceReset += this.DeviceResetHandler;
			this.graphicsDevice.DeviceLost += this.DeviceLostHandler;
			this.UpdateSize();
		}

		#endregion

		#region Public Methods and Operators

		public void DeviceLostHandler(object sender, EventArgs eventArgs)
		{
			GUI.DeviceLost();
		}

		public void PostRender()
		{
			this.deviceState.Save(this.DeviceDX11.ImmediateContext);
			this.uiRenderer.PostRender();
			this.deviceState.Restore(this.DeviceDX11.ImmediateContext);
		}

		public void PreRender(GameTime gameTime)
		{
			GUI.Tick();
			this.uiRenderer.Update(gameTime.TotalGameTime.TotalSeconds);

			this.deviceState.Save(this.DeviceDX11.ImmediateContext);
			this.uiRenderer.PreRender();
			this.deviceState.Restore(this.DeviceDX11.ImmediateContext);
		}

		public void Update(GameTime gameTime)
		{
			this.inputManager.Update();
		}

		#endregion

		#region Methods

		private UIRenderer CreateRenderer(string rootXamlPath, string stylePath)
		{
			if (!string.IsNullOrEmpty(stylePath))
			{
				var theme = (ResourceDictionary)GUI.Load(stylePath);
				GUI.SetTheme(theme);
			}

			var root = (Grid)GUI.Load(rootXamlPath);
			return GUI.CreateRenderer(root);
		}

		private void DeviceResetHandler(object sender, EventArgs e)
		{
			GUI.DeviceReset();
			this.UpdateSize();
		}

		private void UpdateSize()
		{
			var viewport = this.graphicsDevice.Viewport;
			this.uiRenderer.Resize(viewport.Width, viewport.Height);
		}

		private void WindowClientSizeChangedHandler(object sender, EventArgs e)
		{
			this.UpdateSize();
		}

		#endregion
	}
}