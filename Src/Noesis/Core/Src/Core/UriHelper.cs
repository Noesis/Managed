using System;

namespace Noesis
{
    public static class UriHelper
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

        public static string GetAssembly(this Uri uri)
        {
            Uri absUri = GetAbsoluteUri(uri, out _);

            string[] segments = absUri.Segments;
            if (segments.Length > 1 && segments[1].Contains(";component"))
            {
                return segments[1].Substring(0, segments[1].Length - 11);
            }

            return "";
        }

        public static string GetPath(this Uri uri)
        {
            bool isRootPath;
            Uri absUri = GetAbsoluteUri(uri, out isRootPath);

            string path = "";
            if (absUri.IsUnc)
            {
                path += "//" + absUri.Host + "/";
            }
            string[] segments = absUri.Segments;
            if (segments.Length > 1 && !segments[1].Contains(";component"))
            {
                path += isRootPath ? "/" : "";
                path += segments[1];
            }
            for (int i = 2; i < segments.Length; ++i)
            {
                path += segments[i];
            }
            path += absUri.Fragment;

            return Uri.UnescapeDataString(path);
        }

        private static Uri GetAbsoluteUri(Uri uri, out bool isRootPath)
        {
            if (uri.IsAbsoluteUri)
            {
                isRootPath = !uri.IsUnc && uri.OriginalString.StartsWith("file://");
                return uri;
            }

            isRootPath = false;
            return new Uri(new Uri("file://"), uri);
        }
    }
}
