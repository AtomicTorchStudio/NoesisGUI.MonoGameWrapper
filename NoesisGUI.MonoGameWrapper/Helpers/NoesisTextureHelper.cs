namespace NoesisGUI.MonoGameWrapper.Helpers
{
    #region

    using System;

    using Microsoft.Xna.Framework.Graphics;

    using Noesis;

    using Texture = Noesis.Texture;

    #endregion

    public static class NoesisTextureHelper
    {
        #region Public Methods and Operators

        public static Texture CreateNoesisTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            if (texture.IsDisposed)
            {
                throw new Exception("Cannot wrap the disposed texture: " + texture);
            }

            return Texture.WrapD3D11Texture(
                texture,
                texture.GetNativeTexturePtr(),
                texture.Width,
                texture.Height,
                texture.LevelCount,
                format: GetTextureFormat(texture),
                isInverted: false);
        }

        public static TextureSource CreateTextureSource(Texture2D texture)
        {
            return new TextureSource(CreateNoesisTexture(texture));
        }

        #endregion

        #region Methods

        private static IntPtr GetNativeTexturePtr(this Texture2D texture)
        {
            // ai_enabled|AtomicTorch: not implemented because this property is public in our MonoGame framework fork
            // TODO: could be implemented via reflection
            throw new NotImplementedException();
        }

        private static Texture.Format GetTextureFormat(Texture2D texture)
        {
            switch (texture.Format)
            {
                case SurfaceFormat.Color:
                case SurfaceFormat.Bgra32:
                    // BGRA 8 bit per channel, 32 bit total
                    return Texture.Format.BGRA8;

                case SurfaceFormat.Dxt1:
                    return Texture.Format.BC1;

                case SurfaceFormat.Dxt3:
                    return Texture.Format.BC2;

                case SurfaceFormat.Dxt5:
                    return Texture.Format.BC3;

                case SurfaceFormat.Alpha8:
                    // grayscale 8 bit
                    return Texture.Format.R8;
            }

            throw new ArgumentOutOfRangeException("Unknown texture format: " + texture.Format);
        }

        #endregion
    }
}