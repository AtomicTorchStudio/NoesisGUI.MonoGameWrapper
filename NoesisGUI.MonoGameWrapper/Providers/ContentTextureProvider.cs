namespace NoesisGUI.MonoGameWrapper.Providers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Noesis;
    using NoesisGUI.MonoGameWrapper.Helpers;
    using Path = System.IO.Path;
    using Texture = Noesis.Texture;

    /// <summary>
    /// MonoGame Content texture loading provider for NoesisGUI.
    /// </summary>
    public class ContentTextureProvider : TextureProvider, IDisposable
    {
        private readonly Dictionary<string, WeakReference<Texture2D>> cache
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly ContentManager contentManager;

        private readonly string rootPath;

        public ContentTextureProvider(
            ContentManager contentManager,
            string rootPath)
        {
            this.contentManager = contentManager;
            this.rootPath = rootPath;
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

        private Texture2D GetTexture(string filename)
        {
            if (filename.StartsWith(this.rootPath))
            {
                filename = filename.Remove(this.rootPath.Length);
            }

            filename = filename.TrimStart(Path.DirectorySeparatorChar,
                                          Path.AltDirectorySeparatorChar);

            if (this.cache.TryGetValue(filename, out var weakReference)
                && weakReference.TryGetTarget(out var cachedTexture)
                && !cachedTexture.IsDisposed)
            {
                return cachedTexture;
            }

            var texture = this.contentManager.Load<Texture2D>(filename);
            this.cache[filename] = new WeakReference<Texture2D>(texture);
            return texture;
        }
    }
}