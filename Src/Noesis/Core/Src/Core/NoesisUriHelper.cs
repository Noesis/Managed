using System;

namespace Noesis
{
    internal static class UriHelper
    {
        private const string UriSchemePack = "pack";

        public static void RegisterPack()
        {
            if (!UriParser.IsKnownScheme(UriSchemePack))
            {
                UriParser parser = new GenericUriParser(GenericUriParserOptions.GenericAuthority);
                UriParser.Register(parser, UriSchemePack, -1);
            }
        }

        public static string GetPath(Uri uri)
        {
            Uri absUri = GetAbsoluteUri(uri);

            string path = "";
            if (absUri.Segments.Length > 1 && !absUri.Segments[1].Contains(";component"))
            {
                path += absUri.Segments[1];
            }
            for (int i = 2; i < absUri.Segments.Length; ++i)
            {
                path += absUri.Segments[i];
            }

            return path;
        }

        private static Uri GetAbsoluteUri(Uri uri)
        {
            if (uri.IsAbsoluteUri)
            {
                return uri;
            }

            return new Uri(new Uri("http://default.com"), uri);
        }
    }
}
