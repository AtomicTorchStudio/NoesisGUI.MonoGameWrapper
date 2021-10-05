namespace NoesisGUI.MonoGameWrapper.Providers
{
    using System;
    using Noesis;

    public class NoesisProviderManager : IDisposable
    {
        public NoesisProviderManager(
            XamlProvider xamlProvider,
            FontProvider fontProvider,
            TextureProvider textureProvider)
        {
            this.XamlProvider = xamlProvider;
            this.TextureProvider = textureProvider;
            this.FontProvider = fontProvider;
        }

        public FontProvider FontProvider { get; private set; }

        public TextureProvider TextureProvider { get; private set; }

        public XamlProvider XamlProvider { get; private set; }

        public void Dispose()
        {
            (this.XamlProvider as IDisposable)?.Dispose();
            (this.FontProvider as IDisposable)?.Dispose();
            (this.TextureProvider as IDisposable)?.Dispose();
            this.XamlProvider = null;
            this.FontProvider = null;
            this.TextureProvider = null;
        }
    }
}