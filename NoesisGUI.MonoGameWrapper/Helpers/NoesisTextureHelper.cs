namespace NoesisGUI.MonoGameWrapper.Helpers
{
    using System;
    using System.Reflection;
    using Noesis;
    using SharpDX.Direct3D11;
    using Texture = Noesis.Texture;
    using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

    public static class NoesisTextureHelper
    {
        // We need the native texture pointer for the SharpDX texture
        // but this API is not available in MonoGame.Texture class.
        // Here we're using reflection to access the internal method.
        private static readonly MethodInfo GetTextureMethod
            = typeof(Texture2D).GetMethod("GetTexture",
                                          BindingFlags.NonPublic | BindingFlags.Instance);

        public static Texture CreateNoesisTexture(Texture2D texture)
        {
            if (texture is null
                || texture.IsDisposed)
            {
                return null;
            }

            var textureNativePointer = GetTextureNativePointer(texture);

            return RenderDeviceD3D11.WrapTexture(
                texture,
                textureNativePointer,
                texture.Width,
                texture.Height,
                texture.LevelCount,
                isInverted: false,
                hasAlpha: true);
        }

        private static IntPtr GetTextureNativePointer(Texture2D texture)
        {
            var resource = (Resource)GetTextureMethod.Invoke(texture, Array.Empty<object>());
            return resource.NativePointer;
        }
    }
}