namespace NoesisGUI.MonoGameWrapper.Helpers
{
    #region

    using System;
    using System.Reflection;

    using Microsoft.Xna.Framework.Graphics;

    using Texture = Noesis.Texture;

    #endregion

    public static class NoesisTextureHelper
    {
        #region Static Fields

        // This is a hack to access native texture
        private static readonly Lazy<FieldInfo> TextureFieldInfo = new Lazy<FieldInfo>(
            () => typeof(Texture2D).GetField("_texture", BindingFlags.Instance | BindingFlags.NonPublic));

        #endregion

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

            var nativeTexture = (SharpDX.Direct3D11.Texture2D)TextureFieldInfo.Value.GetValue(texture);

            return Texture.WrapD3D11Texture(
                texture,
                nativeTexture.NativePointer,
                texture.Width,
                texture.Height,
                texture.LevelCount,
                format: GetTextureFormat(texture),
                isInverted: false);
        }

        #endregion

        #region Methods

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