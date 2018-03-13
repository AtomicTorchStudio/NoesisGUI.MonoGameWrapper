namespace NoesisGUI.MonoGameWrapper.Input
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Noesis;
    using Keyboard = NoesisGUI.MonoGameWrapper.Input.Devices.Keyboard;
    using Mouse = NoesisGUI.MonoGameWrapper.Input.Devices.Mouse;

    public class InputManager
    {
        private readonly Keyboard keyboard;

        private readonly Mouse mouse;

        internal InputManager(
            View view,
            UIElement uiRendererRoot,
            NoesisConfig config)
        {
            this.keyboard = new Keyboard(
                view,
                uiRendererRoot.Keyboard,
                config);

            this.mouse = new Mouse(view, uiRendererRoot, config);
        }

        /// <summary>
        /// Gets collection of the keyboard keys consumed by NoesisGUI during this frame. You could use it to ignore according
        /// input of the game.
        /// </summary>
        public ICollection<Keys> ConsumedKeyboardKeys => this.keyboard.ConsumedKeys;

        /// <summary>
        /// Gets collection of the mouse buttons consumed by NoesisGUI during this frame. You could use it to ignore according
        /// input of the game.
        /// </summary>
        public ICollection<MouseButton> ConsumedMouseButtons => this.mouse.ConsumedButtons;

        /// <summary>
        /// Gets amount of the mouse wheel delta consumed by NoesisGUI during this frame. You could use it to ignore according
        /// input of the game.
        /// </summary>
        public int ConsumedMouseDeltaWheel => this.mouse.ConsumedDeltaWheel;

        internal void Update(GameTime gameTime, bool isWindowActive)
        {
            this.keyboard.UpdateKeyboard(gameTime, isWindowActive);
            this.mouse.UpdateMouse(gameTime, isWindowActive);
        }
    }
}