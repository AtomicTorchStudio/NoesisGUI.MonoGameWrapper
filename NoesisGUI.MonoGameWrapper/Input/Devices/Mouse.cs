namespace NoesisGUI.MonoGameWrapper.Input.Devices
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Noesis;
    using Point = Noesis.Point;

    internal class Mouse
    {
        public readonly ICollection<MouseButton> ConsumedButtons = new List<MouseButton>();

        private readonly NoesisConfig config;

        private readonly TimeSpan doubleClickInterval;

        private readonly bool isAcceptingMiddleButtonInput;

        /// <summary>
        /// Used for double click handling
        /// </summary>
        private readonly Dictionary<MouseButton, TimeSpan> lastPressTimeDictionary =
            new();

        private readonly Noesis.Keyboard noesisKeyboard;

        private readonly Visual rootVisual;

        private readonly View view;

        private bool isAnyControlUnderMouseCursor;

        private int lastScrollWheelValue;

        private int lastX;

        private int lastY;

        private MouseState previousState;

        private TimeSpan totalGameTime;

        public Mouse(
            View view,
            Visual rootVisual,
            Noesis.Keyboard noesisKeyboard,
            NoesisConfig config)
        {
            this.view = view;
            this.rootVisual = rootVisual;
            this.noesisKeyboard = noesisKeyboard;
            this.config = config;

            this.doubleClickInterval = TimeSpan.FromSeconds(config.InputMouseDoubleClickIntervalSeconds);
            this.isAcceptingMiddleButtonInput = config.IsAcceptingMouseMiddleButtonInput;
        }

        public int ConsumedDeltaWheel { get; private set; }

        public void UpdateMouse(GameTime gameTime, bool isWindowActive)
        {
            this.isAnyControlUnderMouseCursor = this.CheckIsAnyControlUnderMouseCursor();

            this.totalGameTime = gameTime.TotalGameTime;

            if (this.ConsumedButtons.Count > 0)
            {
                this.ConsumedButtons.Clear();
            }

            this.ConsumedDeltaWheel = 0;

            // ReSharper disable once LocalVariableHidesMember
            var previousState = this.previousState;
            MouseState state;
            if (isWindowActive)
            {
                state = Microsoft.Xna.Framework.Input.Mouse.GetState();
            }
            else
            {
                // don't read input if the game window is not focused
                state = new MouseState(
                    x: int.MinValue,
                    y: int.MinValue,
                    scrollWheel: 0,
                    leftButton: ButtonState.Released,
                    rightButton: ButtonState.Released,
                    middleButton: ButtonState.Released,
                    xButton1: ButtonState.Released,
                    xButton2: ButtonState.Released);
            }

            var x = state.X;
            var y = state.Y;
            var viewport = this.config.CallbackGetViewport();
            x -= viewport.X;
            y -= viewport.Y;

            var scrollWheelValue = state.ScrollWheelValue;

            if (this.lastX != x
                || this.lastY != y)
            {
                this.view.MouseMove(x, y);
                this.lastX = x;
                this.lastY = y;
            }

            if (this.lastScrollWheelValue != scrollWheelValue)
            {
                if (this.isAnyControlUnderMouseCursor)
                {
                    var scrollDeltaValue = scrollWheelValue - this.lastScrollWheelValue;
                    // apply workaround to disable horizontal scroll https://www.noesisengine.com/bugs/view.php?id=1457
                    this.view.KeyUp(Key.LeftShift);
                    this.view.KeyUp(Key.RightShift);
                    this.view.MouseWheel(x, y, scrollDeltaValue);
                    this.ConsumedDeltaWheel = scrollDeltaValue;
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

            this.ProcessMouseButtonDown(MouseButton.Left,  state.LeftButton,  previousState.LeftButton);
            this.ProcessMouseButtonDown(MouseButton.Right, state.RightButton, previousState.RightButton);
            if (this.isAcceptingMiddleButtonInput)
            {
                this.ProcessMouseButtonDown(MouseButton.Middle, state.MiddleButton, previousState.MiddleButton);
            }

            this.ProcessMouseButtonUp(MouseButton.Left,  state.LeftButton,  previousState.LeftButton);
            this.ProcessMouseButtonUp(MouseButton.Right, state.RightButton, previousState.RightButton);
            if (this.isAcceptingMiddleButtonInput)
            {
                this.ProcessMouseButtonUp(MouseButton.Middle, state.MiddleButton, previousState.MiddleButton);
            }

            this.previousState = state;
        }

        private bool CheckIsAnyControlUnderMouseCursor()
        {
            DependencyObject hit = null;
            VisualTreeHelper.HitTest(
                this.rootVisual,
                filterCallback: hitCandidate =>
                                {
                                    if (hitCandidate is UIElement uiElement
                                        && (!uiElement.IsHitTestVisible
                                            || !uiElement.IsVisible))
                                    {
                                        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                                    }

                                    return HitTestFilterBehavior.Continue;
                                },
                resultCallback: result =>
                                {
                                    hit = result.VisualHit;
                                    return HitTestResultBehavior.Stop;
                                },
                hitTestParameters: new PointHitTestParameters(new Point(this.lastX, this.lastY)));

            return hit is not null;
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

            this.TryConsumeMouseButton(buttonId);
            if (current == previous)
            {
                // state didn't change
                return;
            }

            var isDoubleClick = false;
            if (buttonId == MouseButton.Left)
            {
                // check double click (NoesisGUI crashes if we check for double click for a mouse button other than the left one)
                this.lastPressTimeDictionary.TryGetValue(buttonId, out var lastPressTime);
                if (this.totalGameTime - lastPressTime < this.doubleClickInterval)
                {
                    //System.Diagnostics.Debug.WriteLine("Mouse double click: " + buttonId);
                    this.view.MouseDoubleClick(this.lastX, this.lastY, buttonId);
                    isDoubleClick = true;
                }
            }

            //System.Diagnostics.Debug.WriteLine("Mouse button down: " + buttonId);
            if (!isDoubleClick)
            {
                this.view.MouseButtonDown(this.lastX, this.lastY, buttonId);
            }

            if (buttonId == MouseButton.Left)
            {
                // record last press time (for double click handling)
                this.lastPressTimeDictionary[buttonId] = this.totalGameTime;
            }
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

            this.TryConsumeMouseButton(buttonId);
            if (current == previous)
            {
                // state didn't change
                return;
            }

            //System.Diagnostics.Debug.WriteLine("Mouse button up: " + buttonId);
            this.view.MouseButtonUp(this.lastX, this.lastY, buttonId);

            if (!this.isAnyControlUnderMouseCursor
                && this.noesisKeyboard.FocusedElement is TextBoxBase)
            {
                // ensure the focus is released
                this.noesisKeyboard.Focus(null);
            }
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