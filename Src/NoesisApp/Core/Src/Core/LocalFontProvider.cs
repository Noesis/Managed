using System;
using System.IO;
using System.Linq;
using Noesis;

namespace NoesisApp
{
    public class LocalFontProvider : FontProvider
    {
        public LocalFontProvider() : this("")
        {
        }

        public LocalFontProvider(string basePath)
        {
            _basePath = basePath;
        }

        public override Stream OpenFont(string folder, string id)
        {
            string path = System.IO.Path.Combine(_basePath, folder, id);
            return new FileStream(path, FileMode.Open);
        }

        public override void ScanFolder(string folder)
        {
            string path = System.IO.Path.Combine(_basePath, folder);
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string extension = System.IO.Path.GetExtension(file);
                if (extension == ".ttf" || extension == ".otf")
                {
                    RegisterFont(folder, System.IO.Path.GetFileName(file));
                }
            }
        }

        private string _basePath;
    }
}
