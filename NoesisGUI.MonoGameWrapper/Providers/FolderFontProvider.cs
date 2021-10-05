namespace NoesisGUI.MonoGameWrapper.Providers
{
    using System;
    using System.IO;
    using Noesis;
    using Path = System.IO.Path;

    public class FolderFontProvider : FontProvider
    {
        private readonly string rootPath;

        public FolderFontProvider(string rootPath)
        {
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                rootPath += Path.DirectorySeparatorChar;
            }

            this.rootPath = rootPath;
        }

        public override Stream OpenFont(string folder, string id)
        {
            var fontPath = id;
            if (File.Exists(fontPath))
            {
                return File.OpenRead(fontPath);
            }

            throw new FileNotFoundException("Font file not found", fontPath);
        }

        public override void ScanFolder(string folder)
        {
            var folderPath = Path.Combine(this.rootPath, folder);
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            var fontFilePaths = Directory.GetFiles(folderPath, "*.*", searchOption: SearchOption.TopDirectoryOnly);
            foreach (var fontPath in fontFilePaths)
            {
                if (fontPath.EndsWith(".ttf",    StringComparison.OrdinalIgnoreCase)
                    || fontPath.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)
                    || fontPath.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase))
                {
                    this.RegisterFont(folder, fontPath);
                }
            }
        }
    }
}