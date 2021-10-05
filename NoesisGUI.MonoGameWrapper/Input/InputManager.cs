namespace NoesisGUI.MonoGameWrapper.Input
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Microsoft.Xna.Framework;
    using Noesis;
    using Keyboard = NoesisGUI.MonoGameWrapper.Input.Devices.Keyboard;
    using Keys = Microsoft.Xna.Framework.Input.Keys;
    using Mouse = NoesisGUI.MonoGameWrapper.Input.Devices.Mouse;

    public class InputManager : IDisposable
    {
        private readonly Keyboard keyboard;

        private readonly Mouse mouse;

        private readonly NoesisViewWrapper viewWrapper;

        internal InputManager(
            NoesisViewWrapper viewWrapper,
            NoesisConfig config,
            Form form)
        {
            this.viewWrapper = viewWrapper;
            var view = viewWrapper.View;
            var controlTreeRoot = view.Content;

            this.keyboard = new Keyboard(
                view,
                controlTreeRoot.Keyboard,
                config,
                form);

            // Find control tree root.
            // It's super important to use global UI root to process visual tree hit testing:
            // controlTreeRoot root is not the UI root and popup visuals are created in the UI root.
            var rootVisual = (Visual)controlTreeRoot;
            while (VisualTreeHelper.GetParent(rootVisual) is Visual parent)
            {
                rootVisual = parent;
            }

            this.mouse = new Mouse(view,
                                   rootVisual,
                                   controlTreeRoot.Keyboard,
                                   config);
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

        public void Dispose()
        {
            this.keyboard?.Dispose();
        }

        public void OnMonoGameChar(char character, Keys key)
        {
            this.keyboard.OnMonoGameChar(character, key);
        }

        public void SoftwareKeyboardCallbackHandler(UIElement focused, bool open)
        {
            this.keyboard.SoftwareKeyboardCallbackHandler(focused, open);
        }

        internal void Update(GameTime gameTime, bool isWindowActive)
        {
            gameTime = this.viewWrapper.CalculateRelativeGameTime(gameTime);

            this.keyboard.UpdateKeyboard(gameTime, isWindowActive);
            this.mouse.UpdateMouse(gameTime, isWindowActive);
        }
    }
}