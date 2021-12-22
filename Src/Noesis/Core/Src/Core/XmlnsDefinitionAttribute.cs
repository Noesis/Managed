using System;

namespace Noesis
{
    /// <summary>
    /// Specifies a mapping on a per-assembly basis between a XAML namespace and a CLR namespace,
    /// which is then used for type resolution by a XAML object writer or XAML schema context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class XmlnsDefinitionAttribute : Attribute
    {
        /// <summary>Gets the XAML namespace identifier specified in this attribute.</summary>
        public string XmlNamespace => _xmlNamespace;

        /// <returns>The CLR namespace, specified as a string.</returns>
        public string ClrNamespace => _clrNamespace;

        /// <summary>Gets or sets the name of the assembly associated with the attribute. </summary>
        public string AssemblyName
        {
            get => _assemblyName;
            set { _assemblyName = value; }
        }

        /// <summary>Default constructor</summary>
        public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
        {
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }
            if (clrNamespace == null)
            {
                throw new ArgumentNullException("clrNamespace");
            }
            _xmlNamespace = xmlNamespace;
            _clrNamespace = clrNamespace;
        }

        #region Private members
        private string _xmlNamespace;
        private string _clrNamespace;
        private string _assemblyName;
        #endregion
    }
}
