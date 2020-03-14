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
        }

        public override Stream LoadXaml(string filename)
        {
            Stream stream = null;

            // First try with user provider
            if (_provider != null)
            {
                stream = _provider.LoadXaml(filename);
            }

            // Next try with application assembly resources
            if (stream == null)
            {
                stream = Application.GetAssemblyResource(_assembly, _namespace, filename);
            }

            // Last try with Theme assembly resources
            if (stream == null)
            {
                stream = Application.GetAssemblyResource(Theme.Assembly, "NoesisApp", filename);
            }

            return stream;
        }

        private Assembly _assembly;
        private string _namespace;
        private XamlProvider _provider;
    }
}
