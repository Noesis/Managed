using System;
using System.IO;
using System.Reflection;
using Noesis;

namespace NoesisApp
{
    public class EmbeddedFontProvider : FontProvider
    {
        public EmbeddedFontProvider(Application app)
        {
            Type type = app.GetType();
            _assembly = type.Assembly;
            _namespace = type.Namespace;
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
            string filename = folder + (folder.Length > 0 ? "/" : "") + id;
            return EmbeddedProviderHelper.GetResource(_assembly, _namespace, filename);
        }

        private Assembly _assembly;
        private string _namespace;
    }
}