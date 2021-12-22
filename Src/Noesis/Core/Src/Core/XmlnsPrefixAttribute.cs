using System;

namespace Noesis
{
    /// <summary>
    /// Identifies a recommended prefix to associate with a XAML namespace for XAML usage,
    /// when writing elements and attributes in a XAML file or when interacting with a
    /// design environment that has XAML editing features.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class XmlnsPrefixAttribute : Attribute
    {
        /// <summary>Gets the XAML namespace identifier associated with this attribute.</summary>
        public string XmlNamespace => _xmlNamespace;

        /// <summary>Gets the recommended prefix associated with this attribute.</summary>
        public string Prefix => _prefix;

        /// <summary>Default constructor</summary>
        public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
        {
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            _xmlNamespace = xmlNamespace;
            _prefix = prefix;
        }

        #region Private members
        private string _xmlNamespace;
        private string _prefix;
        #endregion
    }
}
