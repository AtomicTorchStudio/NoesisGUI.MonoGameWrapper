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

        private readonly NoesisViewWrapper noesisViewWrapper;

        internal InputManager(
            NoesisViewWrapper noesisViewWrapper,
            NoesisConfig config)
        {
            this.noesisViewWrapper = noesisViewWrapper;
            var view = noesisViewWrapper.GetView();
            var controlTreeRoot = view.Content;

            this.keyboard = new Keyboard(
                view,
                controlTreeRoot.Keyboard,
                config);

            // Find control tree root.
            // It's super important to use global UI root to process visual tree hit testing:
            // controlTreeRoot root is not the UI root and popup visuals are created in the UI root.
            var rootVisual = (Visual)controlTreeRoot;
            while (VisualTreeHelper.GetParent(rootVisual) is var parent
                   && parent != null)
            {
                rootVisual = parent;
            }

            this.mouse = new Mouse(view, rootVisual, controlTreeRoot, config);
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
            gameTime = this.noesisViewWrapper.CalculateRelativeGameTime(gameTime);

            this.keyboard.UpdateKeyboard(gameTime, isWindowActive);
            this.mouse.UpdateMouse(gameTime, isWindowActive);
        }
    }
}