using System;
using System.IO;
using System.Reflection;
using Noesis;

namespace NoesisApp
{
    public class EmbeddedXamlProvider : XamlProvider
    {
        public EmbeddedXamlProvider(Assembly assembly, string ns) : this(assembly, ns, null)
        {
        }

        public EmbeddedXamlProvider(Assembly assembly, string ns, XamlProvider provider)
        {
            _assembly = assembly;
            _namespace = ns;
            _provider = provider;

            if (_provider != null)
            {
                _provider.XamlChanged += (uri) =>
                {
                    RaiseXamlChanged(uri);
                };
            }
        }

        public override Stream LoadXaml(Uri uri)
        {
            Stream stream = null;

            // First try with user provider
            if (_provider != null)
            {
                stream = _provider.LoadXaml(uri);
            }

            // Next try with application assembly resources
            if (stream == null)
            {
                stream = Application.GetAssemblyResource(_assembly, _namespace, uri.GetPath());
            }

            // Last try with Theme assembly resources
            if (stream == null)
            {
                stream = Application.GetAssemblyResource(Theme.Assembly, "NoesisApp", uri.GetPath());
            }

            return stream;
        }

        private Assembly _assembly;
        private string _namespace;
        private XamlProvider _provider;
    }
}
