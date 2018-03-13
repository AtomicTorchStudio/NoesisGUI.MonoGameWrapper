namespace NoesisGUI.MonoGameWrapper.Input.Devices
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Noesis;
    using MouseState = Microsoft.Xna.Framework.Input.MouseState;
    using Point = Noesis.Point;

    internal class Mouse
    {
        public readonly ICollection<MouseButton> ConsumedButtons = new List<MouseButton>();

        /// <summary>
        /// Used for excluding consuming of input events by primary LayoutRoot
        /// </summary>
        private readonly HitTestIgnoreDelegate checkIfElementIgnoresHitTest;

        private readonly TimeSpan doubleClickInterval;

        /// <summary>
        /// Used for double click handling
        /// </summary>
        private readonly Dictionary<MouseButton, TimeSpan> lastPressTimeDictionary =
            new Dictionary<MouseButton, TimeSpan>();

        private readonly Visual uiRendererRoot;

        private readonly View view;

        private bool isLastFrameWasScrolled;

        private int lastScrollWheelValue;

        private int lastX;

        private int lastY;

        private MouseState previousMouseState;

        private TimeSpan totalGameTime;

        private bool isAnyControlUnderMouseCursor;

        public Mouse(
            View view,
            Visual uiRendererRoot,
            NoesisConfig config)
        {
            this.view = view;
            this.doubleClickInterval = TimeSpan.FromSeconds(config.InputMouseDoubleClickIntervalSeconds);
            this.uiRendererRoot = uiRendererRoot;
            this.checkIfElementIgnoresHitTest = config.CheckIfElementIgnoresHitTest;
        }

        public int ConsumedDeltaWheel { get; private set; }

        public void UpdateMouse(GameTime gameTime, bool isWindowActive)
        {
            // refresh
            this.isAnyControlUnderMouseCursor = this.CalculateIsAnyControlUnderMouseCursor();

            this.totalGameTime = gameTime.TotalGameTime;

            if (this.ConsumedButtons.Count > 0)
            {
                this.ConsumedButtons.Clear();
            }

            this.ConsumedDeltaWheel = 0;

            MouseState mouseState;
            if (isWindowActive)
            {
                mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            }
            else
            {
                // don't read input if the game window is not focused
                mouseState = new MouseState(
                    this.previousMouseState.X,
                    this.previousMouseState.Y,
                    this.previousMouseState.ScrollWheelValue,
                    leftButton: ButtonState.Released,
                    rightButton: ButtonState.Released,
                    middleButton: ButtonState.Released,
                    xButton1: ButtonState.Released,
                    xButton2: ButtonState.Released);
            }

            var x = mouseState.X;
            var y = mouseState.Y;
            var scrollWheelValue = mouseState.ScrollWheelValue;

            if (this.lastX != x
                || this.lastY != y
                || this.isLastFrameWasScrolled)
            {
                this.view.MouseMove(x, y);
                this.lastX = x;
                this.lastY = y;
                this.isLastFrameWasScrolled = false;
            }

            if (this.lastScrollWheelValue != scrollWheelValue)
            {
                if (this.isAnyControlUnderMouseCursor)
                {
                    var scrollDeltaValue = scrollWheelValue - this.lastScrollWheelValue;
                    this.view.MouseWheel(x, y, scrollDeltaValue);
                    this.ConsumedDeltaWheel = scrollDeltaValue;
                    // on the next frame it's required to update NoesisGUI mouse position
                    // (on the current frame it's doesn't give required affect)
                    this.isLastFrameWasScrolled = true;
                }
                else
                {
                    this.ConsumedDeltaWheel = 0;
                }

                this.lastScrollWheelValue = scrollWheelValue;
            }
            else
            {
                this.ConsumedDeltaWheel = 0;
            }

            this.ProcessMouseButtonDown(MouseButton.Left, mouseState.LeftButton, this.previousMouseState.LeftButton);
            this.ProcessMouseButtonDown(MouseButton.Right, mouseState.RightButton, this.previousMouseState.RightButton);
            this.ProcessMouseButtonDown(
                MouseButton.Middle,
                mouseState.MiddleButton,
                this.previousMouseState.MiddleButton);

            this.ProcessMouseButtonUp(MouseButton.Left, mouseState.LeftButton, this.previousMouseState.LeftButton);
            this.ProcessMouseButtonUp(MouseButton.Right, mouseState.RightButton, this.previousMouseState.RightButton);
            this.ProcessMouseButtonUp(
                MouseButton.Middle,
                mouseState.MiddleButton,
                this.previousMouseState.MiddleButton);

            this.previousMouseState = mouseState;
        }

        private bool CalculateIsAnyControlUnderMouseCursor()
        {
            var visual =
                VisualTreeHelper.HitTest(
                                    this.uiRendererRoot,
                                    new Point(this.lastX, this.lastY))
                                .VisualHit as FrameworkElement;
            while (visual != null)
            {
                if (this.checkIfElementIgnoresHitTest != null
                    && !this.checkIfElementIgnoresHitTest(visual))
                {
                    // hit test successful and is not ignored
                    return true;
                }

                // travel up - maybe the parent control should capture focus
                visual = visual.Parent ?? VisualTreeHelper.GetParent(visual) as FrameworkElement;
                //Console.WriteLine("Is consumed: " + result + " hit: " + visual + " name: " + (visual as FrameworkElement)?.Name);
            }

            return false;
        }

        private void ProcessMouseButtonDown(
            MouseButton buttonId,
            ButtonState current,
            ButtonState previous)
        {
            if (current != ButtonState.Pressed)
            {
                return;
            }

            if (current == previous)
            {
                this.TryConsumeMouseButton(buttonId);
                return;
            }

            //System.Diagnostics.Debug.WriteLine("Mouse button down: " + buttonId);
            this.view.MouseButtonDown(this.lastX, this.lastY, buttonId);
            this.TryConsumeMouseButton(buttonId);
        }

        private void ProcessMouseButtonUp(
            MouseButton buttonId,
            ButtonState current,
            ButtonState previous)
        {
            if (current != ButtonState.Released)
            {
                return;
            }

            if (current == previous)
            {
                this.TryConsumeMouseButton(buttonId);
                return;
            }

            // check double click
            TimeSpan lastPressTime;
            this.lastPressTimeDictionary.TryGetValue(buttonId, out lastPressTime);
            if (this.totalGameTime - lastPressTime < this.doubleClickInterval)
            {
                // System.Diagnostics.Debug.WriteLine("Mouse double click: " + buttonId);
                this.view.MouseDoubleClick(this.lastX, this.lastY, buttonId);
            }

            // System.Diagnostics.Debug.WriteLine("Mouse button up: " + buttonId);
            this.view.MouseButtonUp(this.lastX, this.lastY, buttonId);
            
            // record last press time (for double click handling)
            this.lastPressTimeDictionary[buttonId] = this.totalGameTime;

            this.TryConsumeMouseButton(buttonId);
        }

        private void TryConsumeMouseButton(MouseButton buttonId)
        {
            if (this.isAnyControlUnderMouseCursor)
            {
                // consume!
                this.ConsumedButtons.Add(buttonId);
            }
        }
    }
}