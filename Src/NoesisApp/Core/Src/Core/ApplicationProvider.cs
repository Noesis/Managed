using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Noesis;

namespace NoesisApp
{
    internal class ApplicationProvider
    {
        public ApplicationProvider(Application app, XamlProvider xamlProvider, FontProvider fontProvider, FileTextureProvider texProvider)
        {
            _app = app;
            _xamlProvider = new ApplicationXamlProvider(this, xamlProvider);
            _fontProvider = new ApplicationFontProvider(this, fontProvider);
            _texProvider = new ApplicationTextureProvider(this, texProvider);

            // Register Noesis.Theme assembly as exposed in Blend
            AddAssembly(Theme.Assembly, "Noesis.GUI.Extensions");

            // Register current assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;

                if (!assembly.IsDynamic &&
                    name != "mscorlib" && name != "netstandard" && name != "System" &&
                    !name.StartsWith("System.") && !name.StartsWith("Microsoft."))
                {
                    AddAssembly(assembly, name);
                }
            }
        }

        public XamlProvider XamlProvider { get { return _xamlProvider; } }
        public FontProvider FontProvider { get { return _fontProvider; } }
        public TextureProvider TextureProvider { get { return _texProvider; } }

        public void AddAssembly(Assembly assembly, string name)
        {
            if (!_assemblies.TryGetValue(name, out _))
            {
                _assemblies.Add(name, assembly);

                string[] resources = assembly.GetManifestResourceNames();
                foreach (string resource in resources)
                {
                    _fontProvider.RegisterFontResource(resource, name);
                }
            }
        }

        public Stream GetAssemblyResource(Uri uri)
        {
            string assemblyName = uri.GetAssembly();
            if (string.IsNullOrEmpty(assemblyName))
            {
                _app.LogError(
                    $"Can't load '{uri}', use Uri with assembly when loading XAML files. " +
                    $"For example: '/YourAssembly;component/{uri}'");
                return null;
            }

            Assembly assembly;
            if (!_assemblies.TryGetValue(assemblyName, out assembly))
            {
                _app.LogError($"Can't load '{uri}', assembly '{assemblyName}' not loaded");
                return null;
            }

            Stream stream = null;
            try
            {
                string resource = uri.GetPath().Replace("/", ".");
                stream = assembly.GetManifestResourceStream(resource);

                if (stream == null)
                {
                    _app.LogError(
                        $"Can't find resource name '{resource}' in '{assemblyName}' assembly, " +
                        $"clear <RootNamespace> in your .csproj to embed resources as plain paths");
                }
            }
            catch { }

            return stream;
        }

        private Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();

        private class ApplicationXamlProvider : XamlProvider
        {
            public ApplicationXamlProvider(ApplicationProvider app, XamlProvider user)
            {
                _appProvider = app;
                _userProvider = user;

                if (_userProvider != null)
                {
                    _userProvider.XamlChanged += (uri) =>
                    {
                        RaiseXamlChanged(uri);
                    };
                }
            }

            public override Stream LoadXaml(Uri uri)
            {
                Stream stream = null;

                // First try with user provider
                if (_userProvider != null)
                {
                    stream = _userProvider.LoadXaml(uri);
                }

                // Next try with assembly resources
                if (stream == null)
                {
                    stream = _appProvider.GetAssemblyResource(uri);
                }

                return stream;
            }

            private ApplicationProvider _appProvider;
            private XamlProvider _userProvider;
        }

        private class ApplicationFontProvider : FontProvider
        {
            public ApplicationFontProvider(ApplicationProvider app, FontProvider user)
            {
                _appProvider = app;
                _userProvider = user;

                if (_userProvider != null)
                {
                    _userProvider.FontChanged += (Uri baseUri, string familyName,
                        FontWeight weight, FontStretch stretch, FontStyle style) =>
                    {
                        _userProvider.RaiseFontChanged(baseUri, familyName, weight, stretch, style);
                    };
                }
            }

            public void RegisterFontResource(string resource, string component)
            {
                if (resource.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                    resource.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase) ||
                    resource.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
                {
                    int lastDot = resource.LastIndexOf('.', resource.Length - 5);
                    string folder = lastDot != -1 ? resource.Substring(0, lastDot).Replace('.', '/') : string.Empty;
                    string filename = lastDot != -1 ? resource.Substring(lastDot + 1) : resource;

                    folder = $"/{component};component/{folder}";
                    RegisterFont(new Uri(folder, UriKind.RelativeOrAbsolute), filename);
                }
            }

            public override FontSource MatchFont(Uri baseUri, string familyName,
                ref FontWeight weight, ref FontStretch stretch, ref FontStyle style)
            {
                if (_userProvider != null)
                {
                    FontSource font = _userProvider.MatchFont(baseUri, familyName, ref weight, ref stretch, ref style);
                    if (font.file != null)
                    {
                        return font;
                    }
                }

                return base.MatchFont(baseUri, familyName, ref weight, ref stretch, ref style);
            }

            public override bool FamilyExists(Uri baseUri, string familyName)
            {
                if (_userProvider != null)
                {
                    if (_userProvider.FamilyExists(baseUri, familyName))
                    {
                        return true;
                    }
                }

                return base.FamilyExists(baseUri, familyName);
            }

            public override void ScanFolder(Uri folder)
            {
                if (_userProvider != null)
                {
                    _userProvider.ScanFolder(folder);
                }
            }

            public override Stream OpenFont(Uri folder, string filename)
            {
                Stream stream = null;

                // First try with user provider
                if (_userProvider != null)
                {
                    stream = _userProvider.OpenFont(folder, filename);
                }

                // Next try with application assembly resources
                if (stream == null)
                {
                    string uri = folder.OriginalString;
                    if (uri.Length > 0 && !uri.EndsWith("/")) uri += "/";
                    uri += filename;

                    stream = _appProvider.GetAssemblyResource(new Uri(uri, UriKind.RelativeOrAbsolute));
                }

                return stream;
            }

            private ApplicationProvider _appProvider;
            private FontProvider _userProvider;
        }

        private class ApplicationTextureProvider : FileTextureProvider
        {
            public ApplicationTextureProvider(ApplicationProvider app, FileTextureProvider user)
            {
                _appProvider = app;
                _userProvider = user;

                if (_userProvider != null)
                {
                    _userProvider.TextureChanged += (uri) =>
                    {
                        RaiseTextureChanged(uri);
                    };
                }
            }

            public override Stream OpenStream(Uri uri)
            {
                Stream stream = null;

                if (_userProvider != null)
                {
                    stream = _userProvider.OpenStream(uri);
                }

                // Next try with assembly resources
                if (stream == null)
                {
                    stream = _appProvider.GetAssemblyResource(uri);
                }

                return stream;
            }

            private ApplicationProvider _appProvider;
            private FileTextureProvider _userProvider;
        }

        Application _app;
        ApplicationXamlProvider _xamlProvider;
        ApplicationFontProvider _fontProvider;
        ApplicationTextureProvider _texProvider;
    }
}
