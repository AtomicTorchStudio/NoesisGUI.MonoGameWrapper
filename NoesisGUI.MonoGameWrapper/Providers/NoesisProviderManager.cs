namespace NoesisGUI.MonoGameWrapper.Providers
{
    using System;
    using Noesis;

    public class NoesisProviderManager : IDisposable
    {
        private readonly Provider provider;

        private bool isDisposed;

        public NoesisProviderManager(
            XamlProvider xamlProvider,
            FontProvider fontProvider,
            TextureProvider textureProvider)
        {
            this.provider = new Provider()
            {
                XamlProvider = xamlProvider,
                TextureProvider = textureProvider,
                FontProvider = fontProvider
            };
        }

        ~NoesisProviderManager()
        {
            this.Dispose();
        }

        internal Provider Provider => this.provider;

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            (this.provider.XamlProvider as IDisposable)?.Dispose();
            (this.provider.FontProvider as IDisposable)?.Dispose();
            (this.provider.TextureProvider as IDisposable)?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}