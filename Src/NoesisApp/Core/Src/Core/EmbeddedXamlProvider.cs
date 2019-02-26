using System;
using System.IO;
using System.Reflection;
using Noesis;

namespace NoesisApp
{
    public class EmbeddedXamlProvider : XamlProvider
    {
        public EmbeddedXamlProvider(Application app)
        {
            Type type = app.GetType();
            _assembly = type.Assembly;
            _namespace = type.Namespace;
        }

        public override Stream LoadXaml(string filename)
        {
            return EmbeddedProviderHelper.GetResource(_assembly, _namespace, filename);
        }

        private Assembly _assembly;
        private string _namespace;
    }
}
