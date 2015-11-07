namespace NoesisGUI.MonoGameWrapper.Input
{
	#region

	using System.Collections.Generic;
	using System.Linq;

	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Input;

	using Noesis;

	using MouseState = Microsoft.Xna.Framework.Input.MouseState;

	#endregion

	internal class MonoGameNoesisGUIWrapperInputManager
	{
		#region Fields

		private readonly Keyboard keyboard;

		private readonly Mouse mouse;

		#endregion

		#region Constructors and Destructors

		public MonoGameNoesisGUIWrapperInputManager(UIRenderer uiRenderer)
		{
			this.mouse = new Mouse(uiRenderer);
			this.keyboard = new Keyboard(uiRenderer);
		}

		#endregion

		#region Public Methods and Operators

		public void OnTextInput(TextInputEventArgs args)
		{
			this.keyboard.OnChar(args.Character);

			//var keyDown = (Keys)args.Character;
			//if (keyDown != Keys.None)
			//{
			//	var noesisKey = MonoGameNoesisKeys.Convert(keyDown);
			//	if (noesisKey != Key.None)
			//	{
			//		this.keyboard.OnKeyDown(noesisKey);
			//	}
			//}
		}

		public void Update()
		{
			this.keyboard.UpdateKeyboard();
			this.mouse.UpdateMouse();
		}

		#endregion

		internal class Keyboard
		{
			#region Fields

			private readonly List<Keys> pressedKeys = new List<Keys>();

			private readonly List<Keys> releasedKeys = new List<Keys>();

			private readonly UIRenderer uiRenderer;

			private Keys[] previousKeys = new Keys[0];

			#endregion

			#region Constructors and Destructors

			public Keyboard(UIRenderer uiRenderer)
			{
				this.uiRenderer = uiRenderer;
			}

			#endregion

			#region Public Methods and Operators

			public void OnChar(char character)
			{
				this.uiRenderer.Char(character);
			}

			public void OnKeyDown(Key noesisKey)
			{
				this.uiRenderer.KeyDown(noesisKey);
			}

			public void UpdateKeyboard()
			{
				var state = Microsoft.Xna.Framework.Input.Keyboard.GetState();

				Keys[] currentKeys = state.GetPressedKeys();

				// determine pressed since last update keys
				if (this.pressedKeys.Count > 0)
				{
					this.pressedKeys.Clear();
				}
				foreach (var key in currentKeys)
				{
					if (!this.previousKeys.Contains(key))
					{
						this.pressedKeys.Add(key);
					}
				}

				// determine release since last update keys
				if (this.releasedKeys.Count > 0)
				{
					this.releasedKeys.Clear();
				}
				foreach (var key in this.previousKeys)
				{
					if (!currentKeys.Contains(key))
					{
						this.releasedKeys.Add(key);
					}
				}

				// for each pressed key - KeyDown
				foreach (var keyDown in this.pressedKeys)
				{
					var noesisKey = MonoGameNoesisKeys.Convert(keyDown);
					if (noesisKey != Key.None)
					{
						this.uiRenderer.KeyDown(noesisKey);
					}
				}

				// for each released key - KeyUp
				foreach (var keyUp in this.releasedKeys)
				{
					var noesisKey = MonoGameNoesisKeys.Convert(keyUp);
					if (noesisKey != Key.None)
					{
						this.uiRenderer.KeyUp(noesisKey);
					}
				}

				this.previousKeys = currentKeys;
			}

			#endregion
		}

		internal class Mouse
		{
			#region Fields

			private readonly UIRenderer uiRenderer;

			private int mouseLastX;

			private int mouseLastY;

			private MouseState previousMouseState;

			#endregion

			#region Constructors and Destructors

			public Mouse(UIRenderer uiRenderer)
			{
				this.uiRenderer = uiRenderer;
			}

			#endregion

			#region Public Methods and Operators

			public void UpdateMouse()
			{
				var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
				var x = mouseState.X;
				var y = mouseState.Y;

				if (this.mouseLastX != x
				    || this.mouseLastY != y)
				{
					this.uiRenderer.MouseMove(x, y);
					this.mouseLastX = x;
					this.mouseLastY = y;
				}

				this.ProcessMouseButtonDown(x, y, mouseState.LeftButton, this.previousMouseState.LeftButton, MouseButton.Left);
				this.ProcessMouseButtonDown(x, y, mouseState.RightButton, this.previousMouseState.RightButton, MouseButton.Right);
				this.ProcessMouseButtonDown(x, y, mouseState.MiddleButton, this.previousMouseState.MiddleButton, MouseButton.Middle);

				this.ProcessMouseButtonUp(x, y, mouseState.LeftButton, this.previousMouseState.LeftButton, MouseButton.Left);
				this.ProcessMouseButtonUp(x, y, mouseState.RightButton, this.previousMouseState.RightButton, MouseButton.Right);
				this.ProcessMouseButtonUp(x, y, mouseState.MiddleButton, this.previousMouseState.MiddleButton, MouseButton.Middle);

				this.previousMouseState = mouseState;
			}

			#endregion

			#region Methods

			private void ProcessMouseButtonDown(
				int x,
				int y,
				ButtonState xnaButtonCurrent,
				ButtonState xnaButtonPrevious,
				MouseButton noesisButton)
			{
				if (xnaButtonCurrent == ButtonState.Pressed
				    && xnaButtonCurrent != xnaButtonPrevious)
				{
					this.uiRenderer.MouseDown(x, y, noesisButton);
				}
			}

			private void ProcessMouseButtonUp(
				int x,
				int y,
				ButtonState xnaButtonCurrent,
				ButtonState xnaButtonPrevious,
				MouseButton noesisButton)
			{
				if (xnaButtonCurrent == ButtonState.Released
				    && xnaButtonCurrent != xnaButtonPrevious)
				{
					this.uiRenderer.MouseUp(x, y, noesisButton);
				}
			}

			#endregion
		}
	}
}