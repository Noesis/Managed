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
            bool isRootPath;
            Uri absUri = GetAbsoluteUri(uri, out isRootPath);

            string path = "";
            if (absUri.IsUnc)
            {
                path += "//" + absUri.Host + "/";
            }
            if (absUri.Segments.Length > 1 && !absUri.Segments[1].Contains(";component"))
            {
                path += isRootPath ? "/" : "";
                path += absUri.Segments[1];
            }
            for (int i = 2; i < absUri.Segments.Length; ++i)
            {
                path += absUri.Segments[i];
            }

            return Uri.UnescapeDataString(path);
        }

        private static Uri GetAbsoluteUri(Uri uri, out bool isRootPath)
        {
            Uri uri_ = new Uri(uri.OriginalString, UriKind.RelativeOrAbsolute);

            if (uri_.IsAbsoluteUri)
            {
                isRootPath = false;
                return uri_;
            }

            isRootPath = uri.OriginalString.StartsWith("\\") || uri.OriginalString.StartsWith("/");

            return new Uri(new Uri("file://"), uri);
        }
    }
}
