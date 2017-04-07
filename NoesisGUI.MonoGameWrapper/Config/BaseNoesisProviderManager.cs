namespace NoesisGUI.MonoGameWrapper
{
    using System;
    using Noesis;

    public abstract class BaseNoesisProviderManager : IDisposable
    {
        private readonly Provider provider;

        private bool isDisposed;

        protected BaseNoesisProviderManager(
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

        ~BaseNoesisProviderManager()
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