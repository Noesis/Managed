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

        public override Stream OpenStream(Uri uri)
        {
            string path = System.IO.Path.Combine(_basePath, uri.GetPath());
            if (File.Exists(path))
            {
                return File.OpenRead(path);
            }
            return null;
        }

        private string _basePath;
    }
}
