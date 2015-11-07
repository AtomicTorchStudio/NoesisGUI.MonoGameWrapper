namespace NoesisGUI.MonoGameWrapper.Input
{
	#region

	using System.Collections.Generic;

	using Microsoft.Xna.Framework.Input;

	using Noesis;

	#endregion

	internal static class MonoGameNoesisKeys
	{
		#region Static Fields

		private static readonly Dictionary<Keys, Key> noesisKeys;

		#endregion

		#region Constructors and Destructors

		static MonoGameNoesisKeys()
		{
			noesisKeys = new Dictionary<Keys, Key>();

			noesisKeys.Add(Keys.Back, Key.Back);
			noesisKeys.Add(Keys.Tab, Key.Tab);
			noesisKeys.Add(Keys.OemClear, Key.Clear);
			noesisKeys.Add(Keys.Enter, Key.Return);
			noesisKeys.Add(Keys.Pause, Key.Pause);

			noesisKeys.Add(Keys.Escape, Key.Escape);

			noesisKeys.Add(Keys.Space, Key.Space);
			noesisKeys.Add(Keys.PageUp, Key.Prior); // prior?
			noesisKeys.Add(Keys.PageDown, Key.Next); // next?
			noesisKeys.Add(Keys.End, Key.End);
			noesisKeys.Add(Keys.Home, Key.Home);
			noesisKeys.Add(Keys.Left, Key.Left);
			noesisKeys.Add(Keys.Up, Key.Up);
			noesisKeys.Add(Keys.Right, Key.Right);
			noesisKeys.Add(Keys.Down, Key.Down);
			noesisKeys.Add(Keys.Select, Key.Select);
			noesisKeys.Add(Keys.Print, Key.Print);
			noesisKeys.Add(Keys.Execute, Key.Execute);
			//noesisKeys.Add(Keys.PrintScreen, Key.Print);
			noesisKeys.Add(Keys.Insert, Key.Insert);
			noesisKeys.Add(Keys.Delete, Key.Delete);
			noesisKeys.Add(Keys.Help, Key.Help);

			//noesisKeys.Add(Keys.D0, Key.Alpha0);
			//noesisKeys.Add(Keys.D1, Key.Alpha1);
			//noesisKeys.Add(Keys.D2, Key.Alpha2);
			//noesisKeys.Add(Keys.D3, Key.Alpha3);
			//noesisKeys.Add(Keys.D4, Key.Alpha4);
			//noesisKeys.Add(Keys.D5, Key.Alpha5);
			//noesisKeys.Add(Keys.D6, Key.Alpha6);
			//noesisKeys.Add(Keys.D7, Key.Alpha7);
			//noesisKeys.Add(Keys.D8, Key.Alpha8);
			//noesisKeys.Add(Keys.D9, Key.Alpha9);

			noesisKeys.Add(Keys.NumPad0, Key.Pad0);
			noesisKeys.Add(Keys.NumPad1, Key.Pad1);
			noesisKeys.Add(Keys.NumPad2, Key.Pad2);
			noesisKeys.Add(Keys.NumPad3, Key.Pad3);
			noesisKeys.Add(Keys.NumPad4, Key.Pad4);
			noesisKeys.Add(Keys.NumPad5, Key.Pad5);
			noesisKeys.Add(Keys.NumPad6, Key.Pad6);
			noesisKeys.Add(Keys.NumPad7, Key.Pad7);
			noesisKeys.Add(Keys.NumPad8, Key.Pad8);
			noesisKeys.Add(Keys.NumPad9, Key.Pad9);

			// todo: CHECK THIS
			noesisKeys.Add(Keys.Multiply, Key.Multiply);

			noesisKeys.Add(Keys.OemPlus, Key.Add);
			noesisKeys.Add(Keys.Separator, Key.Separator);
			noesisKeys.Add(Keys.OemMinus, Key.Subtract);
			noesisKeys.Add(Keys.OemPeriod, Key.Decimal);
			noesisKeys.Add(Keys.Divide, Key.Divide);
			//noesisKeys.Add(Keys.KeypadEnter, Key.Return);      // same as Return

			//noesisKeys.Add(Keys.A, Key.A);
			//noesisKeys.Add(Keys.B, Key.B);
			//noesisKeys.Add(Keys.C, Key.C);
			//noesisKeys.Add(Keys.D, Key.D);
			//noesisKeys.Add(Keys.E, Key.E);
			//noesisKeys.Add(Keys.F, Key.F);
			//noesisKeys.Add(Keys.G, Key.G);
			//noesisKeys.Add(Keys.H, Key.H);
			//noesisKeys.Add(Keys.I, Key.I);
			//noesisKeys.Add(Keys.J, Key.J);
			//noesisKeys.Add(Keys.K, Key.K);
			//noesisKeys.Add(Keys.L, Key.L);
			//noesisKeys.Add(Keys.M, Key.M);
			//noesisKeys.Add(Keys.N, Key.N);
			//noesisKeys.Add(Keys.O, Key.O);
			//noesisKeys.Add(Keys.P, Key.P);
			//noesisKeys.Add(Keys.Q, Key.Q);
			//noesisKeys.Add(Keys.R, Key.R);
			//noesisKeys.Add(Keys.S, Key.S);
			//noesisKeys.Add(Keys.T, Key.T);
			//noesisKeys.Add(Keys.U, Key.U);
			//noesisKeys.Add(Keys.V, Key.V);
			//noesisKeys.Add(Keys.W, Key.W);
			//noesisKeys.Add(Keys.X, Key.X);
			//noesisKeys.Add(Keys.Y, Key.Y);
			//noesisKeys.Add(Keys.Z, Key.Z);

			noesisKeys.Add(Keys.F1, Key.F1);
			noesisKeys.Add(Keys.F2, Key.F2);
			noesisKeys.Add(Keys.F3, Key.F3);
			noesisKeys.Add(Keys.F4, Key.F4);
			noesisKeys.Add(Keys.F5, Key.F5);
			noesisKeys.Add(Keys.F6, Key.F6);
			noesisKeys.Add(Keys.F7, Key.F7);
			noesisKeys.Add(Keys.F8, Key.F8);
			noesisKeys.Add(Keys.F9, Key.F9);
			noesisKeys.Add(Keys.F10, Key.F10);
			noesisKeys.Add(Keys.F11, Key.F11);
			noesisKeys.Add(Keys.F12, Key.F12);
			noesisKeys.Add(Keys.F13, Key.F13);
			noesisKeys.Add(Keys.F14, Key.F14);
			noesisKeys.Add(Keys.F15, Key.F15);

			noesisKeys.Add(Keys.NumLock, Key.NumLock);
			noesisKeys.Add(Keys.Scroll, Key.Scroll);

			noesisKeys.Add(Keys.LeftShift, Key.Shift);
			noesisKeys.Add(Keys.RightShift, Key.Shift);

			noesisKeys.Add(Keys.LeftControl, Key.Control);
			noesisKeys.Add(Keys.RightControl, Key.Control);

			noesisKeys.Add(Keys.LeftAlt, Key.Alt);
			noesisKeys.Add(Keys.RightAlt, Key.Alt);
		}

		#endregion

		#region Public Methods and Operators

		public static Key Convert(Keys key)
		{
			Key noesisKey;
			return noesisKeys.TryGetValue(key, out noesisKey) ? noesisKey : Key.None;
		}

		#endregion
	}
}