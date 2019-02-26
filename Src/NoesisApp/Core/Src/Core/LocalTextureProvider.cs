using System;
using System.IO;
using System.Linq;
using Noesis;

namespace NoesisApp
{
    public class LocalTextureProvider : FileTextureProvider
    {
        public LocalTextureProvider() : this("")
        {
        }

        public LocalTextureProvider(string basePath)
        {
            _basePath = basePath;
        }

        public override Stream OpenStream(string filename)
        {
            string path = System.IO.Path.Combine(_basePath, filename);
            return new FileStream(path, FileMode.Open);
        }

        private string _basePath;
    }
}
