using System;
using System.IO;
using System.Reflection;
using Noesis;

namespace NoesisApp
{
    public class EmbeddedTextureProvider : FileTextureProvider
    {
        public EmbeddedTextureProvider(Assembly assembly, string ns) : this(assembly, ns, null)
        {
        }

        public EmbeddedTextureProvider(Assembly assembly, string ns, FileTextureProvider provider)
        {
            _assembly = assembly;
            _namespace = ns;
            _provider = provider;
        }

        public override Stream OpenStream(string filename)
        {
            Stream stream = null;

            if (_provider != null)
            {
                stream = _provider.OpenStream(filename);
            }

            if (stream == null)
            {
                stream = EmbeddedProviderHelper.GetResource(_assembly, _namespace, filename);
            }

            return stream;
        }

        private Assembly _assembly;
        private string _namespace;
        private FileTextureProvider _provider;
    }
}