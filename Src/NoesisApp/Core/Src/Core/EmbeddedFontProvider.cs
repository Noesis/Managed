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
            RegisterFontResources(Theme.Assembly, Theme.Namespace, "Noesis.GUI.Extensions");

            if (_provider != null)
            {
                _provider.FontChanged += (Uri baseUri, string familyName, FontWeight weight,
                    FontStretch stretch, FontStyle style) =>
                {
                    _provider.RaiseFontChanged(baseUri, familyName, weight, stretch, style);
                };
            }
        }

        private void RegisterFontResources(Assembly assembly, string ns, string component = "")
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
                    folder = folder.Replace('\\', '/');
                    if (component.Length > 0)
                    {
                        folder = $"/{component};component/{folder}";
                    }
                    string filename = System.IO.Path.GetFileName(path);
                    RegisterFont(new Uri(folder, UriKind.RelativeOrAbsolute), filename);
                }
            }
        }

        public override void ScanFolder(Uri folder)
        {
        }

        public override Stream OpenFont(Uri folder, string filename)
        {
            Stream stream = null;

            // First try with user provider
            if (_provider != null)
            {
                stream = _provider.OpenFont(folder, filename);
            }

            // Next try with application assembly resources
            if (stream == null)
            {
                string path = folder.GetPath();
                if (path.Length > 0) path += "/";
                path += filename;
                stream = Application.GetAssemblyResource(_assembly, _namespace, path);
            }

            // Last try with Theme assembly resources
            if (stream == null)
            {
                string path = folder.GetPath();
                if (path.Length > 0) path += "/";
                path += filename;
                stream = Application.GetAssemblyResource(Theme.Assembly, "NoesisApp", path);
            }

            return stream;
        }

        private Assembly _assembly;
        private string _namespace;
        private FontProvider _provider;
    }
}
