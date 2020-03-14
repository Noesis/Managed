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

            // Register application font resources
            RegisterFontResources(_assembly, _namespace);

            // Register Theme font resources
            RegisterFontResources(Theme.Assembly, Theme.Namespace);
        }

        private void RegisterFontResources(Assembly assembly, string ns)
        {
            if (assembly == null) return;

            int skipNs = ns.Length + 1;
            char[] pointSeparator = new char[] { '.' };

            foreach (string resource in assembly.GetManifestResourceNames())
            {
                if (resource.EndsWith(".ttf") || resource.EndsWith(".ttc") || resource.EndsWith(".otf"))
                {
                    string[] tokens = resource.Substring(skipNs).Split(pointSeparator);

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

            // First try with user provider
            if (_provider != null)
            {
                stream = _provider.OpenFont(folder, id);
            }

            // Next try with application assembly resources
            if (stream == null)
            {
                string filename = folder + (folder.Length > 0 ? "/" : "") + id;
                stream = Application.GetAssemblyResource(_assembly, _namespace, filename);
            }

            // Last try with Theme assembly resources
            if (stream == null)
            {
                string filename = folder + (folder.Length > 0 ? "/" : "") + id;
                stream = Application.GetAssemblyResource(Theme.Assembly, "NoesisApp", filename);
            }

            return stream;
        }

        private Assembly _assembly;
        private string _namespace;
        private FontProvider _provider;
    }
}
