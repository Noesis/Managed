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

            if (_provider != null)
            {
                stream = _provider.LoadXaml(uri);
            }

            if (stream == null)
            {
                stream = Application.GetAssemblyResource(_assembly, _namespace, uri.GetPath());
            }

            return stream;
        }

        private Assembly _assembly;
        private string _namespace;
        private XamlProvider _provider;
    }
}
