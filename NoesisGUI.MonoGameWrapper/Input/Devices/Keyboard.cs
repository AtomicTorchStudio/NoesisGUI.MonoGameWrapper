namespace NoesisGUI.MonoGameWrapper.Input.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using Microsoft.Xna.Framework;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Helpers;
    using NoesisGUI.MonoGameWrapper.Input.Platforms.Windows;
    using ButtonBase = Noesis.ButtonBase;
    using KeyConverter = NoesisGUI.MonoGameWrapper.Helpers.KeyConverter;
    using Keys = Microsoft.Xna.Framework.Input.Keys;
    using TextBox = Noesis.TextBox;
    using TextBoxBase = Noesis.TextBoxBase;
    using View = Noesis.View;

    internal class Keyboard : IDisposable
    {
        public static readonly Keys[] EmptyKeys = new Keys[0];

        public readonly ICollection<Keys> ConsumedKeys
            = new List<Keys>();

        private readonly Queue<char> charactersQueue
            = new();

        private readonly NoesisConfig config;

        private readonly Dictionary<Keys, double> heldKeysTime
            = new(XnaKeysComparer.Instance);

        private readonly bool isEnableDirectionalNavigation;

        private readonly KeyboardImeWinforms keyboardImeWinforms;

        // remember keys sent to NoesisGUI KeyDown method
        // so these keys will be never ignored for KeyUp event
        private readonly HashSet<Keys> keyDownEventSent
            = new(XnaKeysComparer.Instance);

        private readonly float keyRepeatDelaySeconds;

        private readonly float keyRepeatIntervalSeconds;

        private readonly Noesis.Keyboard noesisKeyboard;

        private readonly View view;

        private HashSet<Keys> currentKeys
            = new(XnaKeysComparer.Instance);

        private Keys lastHeldKey;

        private HashSet<Keys> previousKeys
            = new(XnaKeysComparer.Instance);

        public Keyboard(
            View view,
            Noesis.Keyboard noesisKeyboard,
            NoesisConfig config,
            Form form)
        {
            this.view = view;
            this.noesisKeyboard = noesisKeyboard;
            this.config = config;
            this.isEnableDirectionalNavigation = config.IsEnableDirectionalNavigation;
            this.keyRepeatDelaySeconds = (float)config.InputKeyRepeatDelaySeconds;
            this.keyRepeatIntervalSeconds = (float)config.InputKeyRepeatIntervalSeconds;

            this.keyboardImeWinforms = new KeyboardImeWinforms(form, config.OnErrorMessageReceived);
        }

        public void Dispose()
        {
            this.keyboardImeWinforms?.Dispose();
        }

        public void OnMonoGameChar(char character, Keys key)
        {
            if (this.keyboardImeWinforms.IsImeComposingNow)
            {
                //Debug.WriteLine($"Noesis char event IGNORED (IME mode on) for key {key} - char: {character}");
                return;
            }

            //Debug.WriteLine($"Noesis char event for key {key} - char: {character}");
            this.charactersQueue.Enqueue(character);
        }

        public void SoftwareKeyboardCallbackHandler(UIElement focused, bool open)
        {
            if (!(focused is TextBox))
            {
                return;
            }

            if (open)
            {
                this.keyboardImeWinforms.EnableInput(focused);
            }
            else
            {
                this.keyboardImeWinforms.DisableInput();
            }
        }

        public void UpdateKeyboard(GameTime gameTime, bool isWindowActive)
        {
            if (this.ConsumedKeys.Count > 0)
            {
                this.ConsumedKeys.Clear();
            }

            var state = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            if (this.currentKeys.Count > 0)
            {
                this.currentKeys.Clear();
            }

            Keys[] currentKeysArray;
            if (isWindowActive)
            {
                currentKeysArray = state.GetPressedKeys();
            }
            else
            {
                currentKeysArray = EmptyKeys;
                if (this.charactersQueue.Count > 0)
                {
                    this.charactersQueue.Clear();
                }
            }

            foreach (var key in currentKeysArray)
            {
                this.currentKeys.Add(key);
            }

            try
            {
                // determine the keys pressed since the last update
                foreach (var key in currentKeysArray)
                {
                    this.TryConsumeKey(key);

                    var isHeldKey = this.heldKeysTime.ContainsKey(key);
                    if (!isHeldKey)
                    {
                        // the key is just pressed (initial)
                        this.heldKeysTime[key] = 0;
                        this.OnXnaKeyDown(key);
                        this.lastHeldKey = key;
                    }
                    else if (this.lastHeldKey == key)
                    {
                        // the key is held - try repeat if interval exceeded
                        if (IsKeyModifierOrSpecialKey(key))
                        {
                            // do not repeat modifier key
                            continue;
                        }

                        var heldTime = this.heldKeysTime[key] + gameTime.ElapsedGameTime.TotalSeconds;
                        var keyAccumulatedRepeatsCount = (int)((heldTime - this.keyRepeatDelaySeconds)
                                                               / this.keyRepeatIntervalSeconds);
                        if (keyAccumulatedRepeatsCount > 0)
                        {
                            heldTime -= keyAccumulatedRepeatsCount * this.keyRepeatIntervalSeconds;
                            for (var i = 0; i < keyAccumulatedRepeatsCount; i++)
                            {
                                // invoke key down (repeated)
                                this.OnXnaKeyDown(key);
                            }
                        }

                        // update total key held time
                        this.heldKeysTime[key] = heldTime;
                    }
                }

                this.SendCharEvents();

                try
                {
                    this.keyboardImeWinforms.Update(this.view);
                }
                catch (Exception ex)
                {
                    this.config.OnUnhandledException(ex);
                }

                // determine release since last update keys
                foreach (var key in this.previousKeys)
                {
                    if (this.currentKeys.Contains(key))
                    {
                        // key still pressed
                        continue;
                    }

                    // key released
                    this.heldKeysTime.Remove(key);
                    this.TryConsumeKey(key);
                    this.OnXnaKeyUp(key);
                }
            }
            finally
            {
                // swap current keys <-> previous keys
                var temp = this.previousKeys;
                this.previousKeys = this.currentKeys;
                this.currentKeys = temp;
            }
        }

        // TODO: actually this is necessary only for non-char keys such as arrows.
        // Chars keys should be safely ignored as they're automatically repeat via OnMonoGameChar call.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsKeyModifierOrSpecialKey(Keys key)
        {
            return key == Keys.LeftShift
                   || key == Keys.RightShift
                   || key == Keys.LeftControl
                   || key == Keys.RightControl
                   || key == Keys.LeftAlt
                   || key == Keys.RightAlt
                   || key == Keys.Enter
                   || key == Keys.Space;
        }

        private bool IsKeyIgnored(Keys key, bool isForKeyUp)
        {
            if (isForKeyUp
                && this.keyDownEventSent.Contains(key))
            {
                return false;
            }

            if (this.keyboardImeWinforms.IsImeComposingNow)
            {
                return true;
            }

            var focused = this.noesisKeyboard.FocusedElement;
            if (focused is null)
            {
                // cannot pass key input if there is no focused loaded control
                return true;
            }

            if (this.isEnableDirectionalNavigation)
            {
                // directional (arrow) key navigation is enabled - don't ignore arrow key presses
                return false;
            }

            // check if this is an arrow key
            var isArrowKey = key == Keys.Up
                             || key == Keys.Right
                             || key == Keys.Down
                             || key == Keys.Left;

            if (!isArrowKey)
            {
                return false;
            }

            // it's arrow key - ignore in case a non-textbox is focused
            var isIgnored = !(focused is TextBox);
            return isIgnored;
        }

        private void OnXnaKeyDown(Keys key)
        {
            var noesisKey = KeyConverter.Convert(key);
            if (noesisKey == Key.None)
            {
                return;
            }

            if (this.IsKeyIgnored(key, isForKeyUp: false))
            {
                //Debug.WriteLine("Noesis input: key down IGNORED: " + key);
                return;
            }

            //Debug.WriteLine("Noesis input: key down: " + key);

            try
            {
                this.view.KeyDown(noesisKey);
                this.keyDownEventSent.Add(key);
            }
            catch (Exception ex)
            {
                this.config.OnUnhandledException(ex);
            }
        }

        private void OnXnaKeyUp(Keys key)
        {
            var noesisKey = KeyConverter.Convert(key);
            if (noesisKey == Key.None)
            {
                return;
            }

            if (this.IsKeyIgnored(key, isForKeyUp: true))
            {
                //Debug.WriteLine("Noesis input: key up IGNORED: " + key);
                return;
            }

            //Debug.WriteLine("Noesis input: key up: " + key);

            try
            {
                this.view.KeyUp(noesisKey);
                this.keyDownEventSent.Remove(key);
            }
            catch (Exception ex)
            {
                this.config.OnUnhandledException(ex);
            }

            if (key == Keys.Escape
                && this.noesisKeyboard.FocusedElement is TextBoxBase)
            {
                // ensure the focus is released
                this.noesisKeyboard.Focus(null);
            }
        }

        private void SendCharEvents()
        {
            var str = this.keyboardImeWinforms.TryGetFinishedComposedString();
            if (str is not null)
            {
                //Debug.WriteLine("Noesis input: IME composition input string: " + str);
                foreach (var c in str)
                {
                    this.charactersQueue.Enqueue(c);
                }
            }

            while (this.charactersQueue.Count > 0)
            {
                var character = this.charactersQueue.Dequeue();
                //Debug.WriteLine("Noesis input: char: " + character);

                try
                {
                    this.view.Char(character);
                }
                catch (Exception ex)
                {
                    this.config.OnUnhandledException(ex);
                }
            }
        }

        private void TryConsumeKey(Keys key)
        {
            var focused = this.noesisKeyboard.FocusedElement;
            if (focused == null)
            {
                return;
            }

            if (focused is TextBox)
            {
                // consume!
                this.ConsumedKeys.Add(key);
                return;
            }

            if ((focused is ButtonBase
                 || focused is ComboBoxItem)
                && (key == Keys.Enter
                    || key == Keys.Space
                    || key == Keys.Tab))
            {
                // consume!
                this.ConsumedKeys.Add(key);
            }
        }
    }
}