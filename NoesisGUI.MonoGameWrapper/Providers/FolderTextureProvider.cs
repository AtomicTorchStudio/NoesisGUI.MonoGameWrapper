namespace NoesisGUI.MonoGameWrapper.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Helpers;
    using Color = Microsoft.Xna.Framework.Color;
    using Path = System.IO.Path;
    using Texture = Noesis.Texture;

    /// <summary>
    /// Default texture loading provider for NoesisGUI.
    /// Please note this is a very unefficient loader as simply added as a proof of work.
    /// You might want to replace it with <see cref="ContentTextureProvider" /> as the much more efficient solution.
    /// </summary>
    public class FolderTextureProvider : TextureProvider, IDisposable
    {
        private readonly Dictionary<string, WeakReference<Texture2D>> cache
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly GraphicsDevice graphicsDevice;

        private readonly string rootPath;

        public FolderTextureProvider(string rootPath, GraphicsDevice graphicsDevice)
        {
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                rootPath += Path.DirectorySeparatorChar;
            }

            this.rootPath = rootPath;
            this.graphicsDevice = graphicsDevice;
        }

        public void Dispose()
        {
            foreach (var entry in this.cache)
            {
                if (entry.Value.TryGetTarget(out var texture))
                {
                    texture.Dispose();
                }
            }

            this.cache.Clear();
        }

        public override TextureInfo GetTextureInfo(Uri uri)
        {
            var filename = uri.GetPath();
            var texture = this.GetTexture(filename);
            return new TextureInfo(texture.Width, texture.Height);
        }

        public override Texture LoadTexture(Uri uri)
        {
            var filename = uri.GetPath();
            var texture2D = this.GetTexture(filename);
            return NoesisTextureHelper.CreateNoesisTexture(texture2D);
        }

        protected virtual Texture2D LoadTextureFromStream(FileStream fileStream)
        {
            var texture = Texture2D.FromStream(this.graphicsDevice, fileStream);
            if (texture.Format != SurfaceFormat.Color)
            {
                return texture;
            }

            // unfortunately, MonoGame loads textures as non-premultiplied alpha
            // need to premultiply alpha for correct rendering with NoesisGUI
            var buffer = new Color[texture.Width * texture.Height];
            texture.GetData(buffer);
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = Color.FromNonPremultiplied(
                    buffer[i].R,
                    buffer[i].G,
                    buffer[i].B,
                    buffer[i].A);
            }

            texture.SetData(buffer);
            return texture;
        }

        private Texture2D GetTexture(string filename)
        {
            if (this.cache.TryGetValue(filename, out var weakReference)
                && weakReference.TryGetTarget(out var cachedTexture)
                && !cachedTexture.IsDisposed)
            {
                return cachedTexture;
            }

            var fullPath = Path.Combine(this.rootPath, filename);
            using (var fileStream = File.OpenRead(fullPath))
            {
                var texture = this.LoadTextureFromStream(fileStream);
                this.cache[filename] = new WeakReference<Texture2D>(texture);
                return texture;
            }
        }
    }
}