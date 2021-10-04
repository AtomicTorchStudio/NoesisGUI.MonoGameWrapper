namespace NoesisGUI.MonoGameWrapper.Helpers
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Input;

    internal class XnaKeysComparer : IEqualityComparer<Keys>
    {
        public static readonly XnaKeysComparer Instance = new();

        private XnaKeysComparer()
        {
        }

        public bool Equals(Keys x, Keys y)
        {
            return x == y;
        }

        public int GetHashCode(Keys obj)
        {
            return (int)obj;
        }
    }
}