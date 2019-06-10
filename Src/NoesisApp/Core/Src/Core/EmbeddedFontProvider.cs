using System;
using System.IO;
using System.Reflection;
using Noesis;

namespace NoesisApp
{
    public class EmbeddedFontProvider : FontProvider
    {
        public EmbeddedFontProvider(Assembly assembly, string ns) : this(assembly, ns, null)
        {
        }

        public EmbeddedFontProvider(Assembly assembly, string ns, FontProvider provider)
        {
            _assembly = assembly;
            _namespace = ns;
            _provider = provider;

            char[] pointSeparator = new char[] { '.' };

            foreach (string resource in _assembly.GetManifestResourceNames())
            {
                if (resource.EndsWith(".ttf") || resource.EndsWith(".ttc") || resource.EndsWith(".otf"))
                {
                    string[] tokens = resource.Substring(_namespace.Length + 1).Split(pointSeparator);

                    string path = tokens[0];
                    for (int i = 1; i < tokens.Length - 1; ++i)
                    {
                        path += System.IO.Path.DirectorySeparatorChar;
                        path += tokens[i];
                    }
                    path += ".";
                    path += tokens[tokens.Length - 1];

                    string folder = System.IO.Path.GetDirectoryName(path);
                    string id = System.IO.Path.GetFileName(path);
                    RegisterFont(folder.Replace('\\', '/'), id);
                }
            }
        }

        public override void ScanFolder(string folder)
        {
        }

        public override Stream OpenFont(string folder, string id)
        {
            Stream stream = null;

            if (_provider != null)
            {
                stream = _provider.OpenFont(folder, id);
            }

            if (stream == null)
            {
                string filename = folder + (folder.Length > 0 ? "/" : "") + id;
                stream = EmbeddedProviderHelper.GetResource(_assembly, _namespace, filename);
            }

            return stream;
        }

        private Assembly _assembly;
        private string _namespace;
        private FontProvider _provider;
    }
}