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

            // Initialize the entry for all resources list
            Dictionary<string, ResourceInfo> allResources = new Dictionary<string, ResourceInfo>();
            _resources.Add("", allResources);

            // Register MessageBox xaml
            allResources.Add("Src.Core.MessageBox.xaml", new ResourceInfo
            {
                Assembly = typeof(Application).Assembly,
                Name = "NoesisApp.Src.Core.MessageBox.xaml"
            });

            // Register current assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;

                if (!assembly.IsDynamic &&
                    name != "mscorlib" && name != "netstandard" && name != "System" &&
                    !name.StartsWith("System.") && !name.StartsWith("Microsoft.") &&
                    !name.StartsWith("Noesis."))
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
            if (_resources.TryGetValue(name, out _)) return; // assembly already registered

            string[] resources = assembly.GetManifestResourceNames();
            if (resources.Length == 0) return; // assembly doesn't have resources

            string ns = FindRootNamespace(assembly, resources);
            if (string.IsNullOrEmpty(ns)) return; // can't find the root namespace for this assembly

            AddAssembly(assembly, ns, name, resources);
        }

        private void AddAssembly(Assembly assembly, string ns, string component, string[] resources)
        {
            // We will add each resource to 2 lists:
            //  - one containing all resources so we can search when the assembly is not specified in the uri
            //  - one for each assembly, so we can do a fast search when the assembly is specified in the uri
            Dictionary<string, ResourceInfo> allResources = _resources[""];
            Dictionary<string, ResourceInfo> assemblyResources = new Dictionary<string, ResourceInfo>();
            _resources.Add(component, assemblyResources);

            foreach (string name in resources)
            {
                string resource = name.Remove(0, ns.Length + 1);

                ResourceInfo info = new ResourceInfo { Assembly = assembly, Name = name };
                if (!allResources.TryGetValue(resource, out ResourceInfo existingInfo))
                {
                    allResources.Add(resource, info);
                }
                else
                {
                    string existingAssembly = existingInfo.Assembly.GetName().Name;
                    _app.LogWarning($"Resource '{resource}' exists in assemblies '{existingAssembly}' and '{component}', " +
                        "you should specify the assembly when loading this resource");
                }

                assemblyResources.Add(resource, info);

                _fontProvider.RegisterFontResource(resource, component);
            }
        }

        private string FindRootNamespace(Assembly assembly, string[] resources)
        {
            IEnumerable<string> types = assembly.GetExportedTypes().Select(t => t.Namespace).Distinct();
            return FindRootNamespace(assembly, types.Concat(resources));
        }

        private string FindRootNamespace(Assembly assembly, IEnumerable<string> names)
        {
            string ns = names.OrderBy(x => x.Length).FirstOrDefault();

            while (true)
            {
                string nsdot = ns + ".";
                if (names.All(n => n == ns || n.StartsWith(nsdot)))
                {
                    return ns;
                }

                int pos = ns.LastIndexOf('.');
                if (pos == -1)
                {
                    _app.LogWarning($"Can't register embedded resources for '{assembly.GetName().Name}', " +
                        "no common namespace in types and resources found");
                    return string.Empty;
                }

                ns = ns.Substring(0, pos);
            }
        }

        public Stream GetAssemblyResource(Uri uri)
        {
            string asm = uri.GetAssembly();
            Dictionary<string, ResourceInfo> resources;
            if (!_resources.TryGetValue(asm, out resources)) return null;

            string path = uri.GetPath().Replace('/', '.');
            ResourceInfo info;
            if (!resources.TryGetValue(path, out info)) return null;

            return info.Assembly.GetManifestResourceStream(info.Name);
        }

        private struct ResourceInfo
        {
            public Assembly Assembly;
            public string Name;
        }

        private Dictionary<string, Dictionary<string, ResourceInfo>> _resources =
            new Dictionary<string, Dictionary<string, ResourceInfo>>();

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

                // Last try with Theme resources
                if (stream == null)
                {
                    stream = Application.GetAssemblyResource(Theme.Assembly, Theme.Namespace, uri.GetPath());
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

                RegisterThemeFonts();

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

                    RegisterFont(new Uri(folder, UriKind.RelativeOrAbsolute), filename);

                    folder = $"/{component};component/{folder}";
                    RegisterFont(new Uri(folder, UriKind.RelativeOrAbsolute), filename);
                }
            }

            private void RegisterThemeFonts()
            {
                foreach (string name in Theme.Assembly.GetManifestResourceNames())
                {
                    string resource = name.Remove(0, Theme.Namespace.Length + 1);
                    RegisterFontResource(resource, "Noesis.GUI.Extensions");
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

                // Last try with Theme resources
                if (stream == null)
                {
                    string path = folder.GetPath();
                    if (path.Length > 0 && !path.EndsWith("/")) path += "/";
                    path += filename;

                    stream = Application.GetAssemblyResource(Theme.Assembly, Theme.Namespace, path);
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

                // Last try with Theme resources
                if (stream == null)
                {
                    stream = Application.GetAssemblyResource(Theme.Assembly, Theme.Namespace, uri.GetPath());
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
