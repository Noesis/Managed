using System;
using System.IO;
using Noesis;

namespace NoesisApp
{
    public class LocalXamlProvider : XamlProvider
    {
        public LocalXamlProvider() : this("")
        {
        }

        public LocalXamlProvider(string basePath)
        {
            _basePath = basePath;
        }

        public override Stream LoadXaml(Uri uri)
        {
            string path = System.IO.Path.Combine(_basePath, uri.GetPath());
            return new FileStream(path, FileMode.Open);
        }

        private string _basePath;
    }
}
