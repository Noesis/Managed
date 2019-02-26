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

        public override Stream LoadXaml(string filename)
        {
            string path = System.IO.Path.Combine(_basePath, filename);
            return new FileStream(path, FileMode.Open);
        }

        private string _basePath;
    }
}
