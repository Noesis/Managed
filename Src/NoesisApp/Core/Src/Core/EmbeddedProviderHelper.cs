using System;
using System.IO;
using System.Reflection;
using Noesis;

namespace NoesisApp
{
    public static class EmbeddedProviderHelper
    {
        public static Stream GetResource(Assembly assembly, string ns, string filename)
        {
            string resource = ns + "." + filename.Replace("/", ".");
            try
            {
                return assembly.GetManifestResourceStream(resource);
            }
            catch
            {
                return null;
            }
        }
    }
}