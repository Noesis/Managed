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

            if (_provider != null)
            {
                _provider.TextureChanged += (uri) =>
                {
                    RaiseTextureChanged(uri);
                };
            }
        }

        public override Stream OpenStream(Uri uri)
        {
            Stream stream = null;

            if (_provider != null)
            {
                stream = _provider.OpenStream(uri);
            }

            if (stream == null)
            {
                stream = Application.GetAssemblyResource(_assembly, _namespace, uri.GetPath());
            }

            return stream;
        }

        private Assembly _assembly;
        private string _namespace;
        private FileTextureProvider _provider;
    }
}