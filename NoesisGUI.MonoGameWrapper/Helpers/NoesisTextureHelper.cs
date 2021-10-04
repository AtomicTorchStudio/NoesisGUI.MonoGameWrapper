namespace NoesisGUI.MonoGameWrapper.Helpers
{
    using System;
    using Microsoft.Xna.Framework.Graphics;
    using Texture = Noesis.Texture;

    public static class NoesisTextureHelper
    {
        public static Texture CreateNoesisTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            if (texture.IsDisposed)
            {
                return null;

                throw new Exception("Cannot wrap the disposed texture: " + texture);
            }

            return Texture.WrapD3D11Texture(
                texture,
                // TODO: we need the native texture pointer here but this API is not available with NoesisGUI NuGet
                //texture.GetNativeTexturePtr(),
                texture.GetSharedHandle(), // likely this will not work as expected
                texture.Width,
                texture.Height,
                texture.LevelCount,
                isInverted: false);
        }
    }
}