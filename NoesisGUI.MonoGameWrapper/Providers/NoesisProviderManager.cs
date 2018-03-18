namespace NoesisGUI.MonoGameWrapper.Providers
{
    using System;
    using Noesis;

    public class NoesisProviderManager : IDisposable
    {
        private Provider provider;

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

        internal Provider Provider => this.provider;

        public void Dispose()
        {
            if (this.provider == null)
            {
                return;
            }

            (this.provider.XamlProvider as IDisposable)?.Dispose();
            (this.provider.FontProvider as IDisposable)?.Dispose();
            (this.provider.TextureProvider as IDisposable)?.Dispose();
            this.provider = null;
        }
    }
}