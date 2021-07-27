using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    /// <summary>
    /// Represents a service that reports situational object-property relationships for markup extension evaluation.
    /// </summary>
    public interface IProvideValueTarget
    {
        /// <summary>
        /// Gets the target object being reported.
        /// </summary>
        object TargetObject { get; }

        /// <summary>
        /// Gets an identifier for the target property being reported.
        /// </summary>
        object TargetProperty { get; }
    }

    /// <summary>
    /// Represents a service that can use application context to resolve a provided relative URI to an absolute URI.
    /// </summary>
    public interface IUriContext
    {
        /// <summary>
        /// Gets or sets the base URI of the current application context.
        /// </summary>
        Uri BaseUri { get; }
    }

    /// <summary>
    /// Represents a service that resolves from named elements in XAML markup to the appropriate type.
    /// </summary>
    public interface IXamlTypeResolver
    {
        /// <summary>
        /// Resolves a named XAML type to the corresponding Type. The type name is optionally qualified
        /// by the prefix for a XML namespace, otherwise the current default XML namespace is assumed.
        /// </summary>
        Type Resolve(string qualifiedTypeName);
    }

    /// <summary>
    /// Describes a service that can return a XAML namespace that is based on its prefix as it is mapped in XAML markup.
    /// </summary>
    public interface IXamlNamespaceResolver
    {
        /// <summary>
        /// Retrieves a XAML namespace identifier for the specified prefix string.
        /// </summary>
        string GetNamespace(string prefix);
    }

    /// <summary>
    /// Describes a service that can return objects that are specified by XAML name, or alternatively, returns a token that defers name resolution. The service can also return an enumerable set of all named objects that are in the XAML namescope.
    /// </summary>
    public interface IXamlNameResolver
    {
        /// <summary>
        /// Resolves an object from a name reference.
        /// </summary>
        object Resolve(string name);
    }

    /// <summary>
    /// Implementation of XAML service providers available for MarkupExtensions
    /// </summary>
    internal class MarkupExtensionProvider : IServiceProvider, IProvideValueTarget, IUriContext, IXamlTypeResolver, IXamlNamespaceResolver, IXamlNameResolver
    {
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
            {
                return this;
            }
            if (serviceType == typeof(IUriContext))
            {
                return this;
            }
            if (serviceType == typeof(IXamlTypeResolver))
            {
                return this;
            }
            if (serviceType == typeof(IXamlNamespaceResolver))
            {
                return this;
            }
            if (serviceType == typeof(IXamlNameResolver))
            {
                return this;
            }

            return null;
        }

        object IProvideValueTarget.TargetObject
        {
            get
            {
                IntPtr targetPtr = MarkupExtensionProvider_TargetObject(_provider);
                return Extend.GetProxy(targetPtr, false);
            }
        }

        object IProvideValueTarget.TargetProperty
        {
            get
            {
                IntPtr propPtr = MarkupExtensionProvider_TargetProperty(_provider);
                return Extend.GetProxy(propPtr, false);
            }
        }

        Uri IUriContext.BaseUri
        {
            get
            {
                IntPtr uriPtr = MarkupExtensionProvider_BaseUri(_provider);
                string uri = Extend.StringFromNativeUtf8(uriPtr);
                return new Uri(uri, UriKind.RelativeOrAbsolute);
            }
        }

        Type IXamlTypeResolver.Resolve(string qualifiedTypeName)
        {
            IntPtr cPtr = MarkupExtensionProvider_ResolveType(_provider, qualifiedTypeName);
            if (cPtr != IntPtr.Zero)
            {
                Extend.NativeTypeInfo info = Extend.GetNativeTypeInfo(cPtr);
                return info.Type;
            }
            return null;
        }

        string IXamlNamespaceResolver.GetNamespace(string prefix)
        {
            IntPtr strPtr = MarkupExtensionProvider_GetNamespace(_provider, prefix);
            return Extend.StringFromNativeUtf8(strPtr);
        }

        object IXamlNameResolver.Resolve(string name)
        {
            IntPtr objectPtr = MarkupExtensionProvider_ResolveName(_provider, name);
            return Extend.GetProxy(objectPtr, false);
        }

        #region Private members

        internal MarkupExtensionProvider(IntPtr cPtr)
        {
            _provider = new HandleRef(this, cPtr);
        }

        private HandleRef _provider;

        [DllImport(Library.Name)]
        static extern IntPtr MarkupExtensionProvider_TargetObject(HandleRef provider);

        [DllImport(Library.Name)]
        static extern IntPtr MarkupExtensionProvider_TargetProperty(HandleRef provider);

        [DllImport(Library.Name)]
        static extern IntPtr MarkupExtensionProvider_BaseUri(HandleRef provider);

        [DllImport(Library.Name)]
        static extern IntPtr MarkupExtensionProvider_ResolveType(HandleRef provider, [MarshalAs(UnmanagedType.LPWStr)]string name);

        [DllImport(Library.Name)]
        static extern IntPtr MarkupExtensionProvider_GetNamespace(HandleRef provider, [MarshalAs(UnmanagedType.LPWStr)]string name);

        [DllImport(Library.Name)]
        static extern IntPtr MarkupExtensionProvider_ResolveName(HandleRef provider, [MarshalAs(UnmanagedType.LPWStr)]string name);

        #endregion
    }
}
