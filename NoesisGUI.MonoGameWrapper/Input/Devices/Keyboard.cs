namespace NoesisGUI.MonoGameWrapper.Input.Devices
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Helpers;
    using NoesisGUI.MonoGameWrapper.Input.VirtualKeys;

    internal class Keyboard
    {
        public static readonly Keys[] EmptyKeys = new Keys[0];

        private static readonly InputVirtualKeyHelper VirtualKeyHelper = InputVirtualKeyHelper.GetPlatform();

        public readonly ICollection<Keys> ConsumedKeys = new List<Keys>();

        private readonly Dictionary<Keys, float> heldKeysTime = new Dictionary<Keys, float>(XnaKeysComparer.Instance);

        private readonly float keyRepeatDelaySeconds;

        private readonly float keyRepeatIntervalSeconds;

        private readonly Noesis.Keyboard noesisKeyboard;

        private readonly View view;

        private HashSet<Keys> currentKeys = new HashSet<Keys>(XnaKeysComparer.Instance);

        private HashSet<Keys> previousKeys = new HashSet<Keys>(XnaKeysComparer.Instance);

        public Keyboard(
            View view,
            Noesis.Keyboard noesisKeyboard,
            NoesisConfig config)
        {
            this.view = view;
            this.noesisKeyboard = noesisKeyboard;
            this.keyRepeatDelaySeconds = (float)config.InputKeyRepeatDelaySeconds;
            this.keyRepeatIntervalSeconds = (float)config.InputKeyRepeatIntervalSeconds;
        }

        public void UpdateKeyboard(GameTime gameTime, bool isWindowActive)
        {
            if (this.ConsumedKeys.Count > 0)
            {
                this.ConsumedKeys.Clear();
            }

            var totalSecondsElapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var state = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            if (this.currentKeys.Count > 0)
            {
                this.currentKeys.Clear();
            }

            var currentKeysArray = isWindowActive ? state.GetPressedKeys() : EmptyKeys;
            foreach (var key in currentKeysArray)
            {
                this.currentKeys.Add(key);
            }

            // determine pressed since last update keys
            foreach (var key in this.currentKeys)
            {
                this.TryConsumeKey(key);

                var isHeldKey = this.heldKeysTime.ContainsKey(key);
                if (!isHeldKey)
                {
                    this.heldKeysTime[key] = 0;
                    // invoke key down (initial)
                    this.OnXnaKeyDown(key);
                }
                else
                {
                    // the key is held - try repeat if interval exceeded
                    if (IsKeyModifier(key))
                    {
                        // do not repeat modifier key
                        continue;
                    }

                    var heldTime = this.heldKeysTime[key] + totalSecondsElapsed;
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

            // swap current keys <-> previous keys
            var temp = this.previousKeys;
            this.previousKeys = this.currentKeys;
            this.currentKeys = temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsKeyModifier(Keys key)
        {
            return key == Keys.LeftShift
                   || key == Keys.RightShift
                   || key == Keys.LeftControl
                   || key == Keys.RightControl
                   || key == Keys.LeftAlt
                   || key == Keys.RightAlt;
        }

        private void OnXnaKeyDown(Keys key)
        {
            var noesisKey = KeyConverter.Convert(key);
            if (noesisKey == Key.None)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("Noesis key down: " + noesisKey);
            this.view.KeyDown(noesisKey);

            var keyChar = VirtualKeyHelper.KeyCodeToUnicode((System.Windows.Forms.Keys)key);
            if (keyChar.Length != 1
                || keyChar[0] == '\0')
            {
                // no char pressed or unknown char pressed
                return;
            }

            //System.Diagnostics.Debug.WriteLine("Noesis char: " + keyChar[0]);
            this.view.Char(keyChar[0]);
        }

        private void OnXnaKeyUp(Keys key)
        {
            var noesisKey = KeyConverter.Convert(key);
            if (noesisKey == Key.None)
            {
                return;
            }

            //System.Diagnostics.Debug.WriteLine("Noesis key up: " + noesisKey);
            this.view.KeyUp(noesisKey);
        }

        private void TryConsumeKey(Keys key)
        {
            var focused = this.noesisKeyboard.GetFocused();
            if (focused == null)
            {
                return;
            }

            if (focused is TextBox)
            {
                // consume!
                this.ConsumedKeys.Add(key);
            }

            if ((focused is ButtonBase
                 || focused is ComboBoxItem)
                && (key == Keys.Enter
                    || key == Keys.Space
                    || key == Keys.Tab
                    || key == Keys.Left
                    || key == Keys.Up
                    || key == Keys.Right
                    || key == Keys.Down))
            {
                // consume!
                this.ConsumedKeys.Add(key);
            }
        }
    }
}