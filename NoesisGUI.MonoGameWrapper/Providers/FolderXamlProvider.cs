namespace NoesisGUI.MonoGameWrapper.Providers
{
    using System;
    using System.IO;
    using Noesis;
    using Path = System.IO.Path;

    public class FolderXamlProvider : XamlProvider
    {
        private readonly string rootPath;

        public FolderXamlProvider(string rootPath)
        {
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                rootPath += Path.DirectorySeparatorChar;
            }

            this.rootPath = rootPath;
        }

        public override Stream LoadXaml(Uri uri)
        {
            var filename = uri.GetPath();
            var fullPath = Path.Combine(this.rootPath, filename);
            return File.OpenRead(fullPath);
        }
    }
}