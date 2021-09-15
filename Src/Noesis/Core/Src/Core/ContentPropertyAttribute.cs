using System;

namespace Noesis
{
    /// <summary>Indicates which property of a type is the XAML content property. A XAML processor
    /// uses this information when processing XAML child elements of XAML representations of the
    /// attributed type.</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ContentPropertyAttribute : Attribute
    {
        private string _name;

        /// <summary>Gets the name of the property that is the content property.</summary>
        /// <returns>The name of the property that is the content property.</returns>
        public string Name { get{ return this._name; } }

        /// <summary>Initializes a new instance of the ContentPropertyAttribute class.</summary>
        public ContentPropertyAttribute() { }

        /// <summary>Initializes a new instance of the ContentPropertyAttribute class, by using the
        /// specified name.</summary>
        /// <param name="name">The property name for the property that is the content
        /// property.</param>
        public ContentPropertyAttribute(string name) { this._name = name; }
    }
}
