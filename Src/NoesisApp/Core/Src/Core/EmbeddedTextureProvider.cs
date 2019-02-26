using System;
using System.IO;
using System.Reflection;
using Noesis;

namespace NoesisApp
{
    public class EmbeddedTextureProvider : FileTextureProvider
    {
        public EmbeddedTextureProvider(Application app)
        {
            Type type = app.GetType();
            _assembly = type.Assembly;
            _namespace = type.Namespace;
        }

        public override Stream OpenStream(string filename)
        {
            return EmbeddedProviderHelper.GetResource(_assembly, _namespace, filename);
        }

        private Assembly _assembly;
        private string _namespace;
    }
}