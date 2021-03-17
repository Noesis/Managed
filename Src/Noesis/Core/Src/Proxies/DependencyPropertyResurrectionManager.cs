namespace Noesis
{
    using System;
    using System.Collections.Generic;

    public class DependencyPropertyResurrectionManager
    {
        private static readonly Dictionary<WeakTypeEntry, List<PropertyData>> Dictionary
            = new Dictionary<WeakTypeEntry, List<PropertyData>>();

        public static void Register(
            DependencyProperty dependencyProperty,
            string name,
            Type propertyType,
            Type ownerType,
            PropertyMetadata propertyMetadata)
        {
            var weakTypeEntry = new WeakTypeEntry(ownerType);
            if (!Dictionary.TryGetValue(weakTypeEntry, out var properties))
            {
                properties = new List<PropertyData>();
                Dictionary[weakTypeEntry] = properties;
            }
            else
            {
                foreach (var property in properties)
                {
                    if (name == property.Name)
                    {
                        // already registered
                        return;
                    }
                }
            }

            properties.Add(
                new PropertyData(name,
                                 propertyType,
                                 propertyMetadata,
                                 dependencyProperty));
        }

        public static bool TryResurrect(
            IntPtr cPtr,
            string name,
            Type ownerType,
            out DependencyProperty dependencyProperty)
        {
            var weakTypeEntry = new WeakTypeEntry(ownerType);
            if (!Dictionary.TryGetValue(weakTypeEntry, out var properties))
            {
                dependencyProperty = null;
                return false;
            }

            foreach (var property in properties)
            {
                if (name != property.Name)
                {
                    continue;
                }

                property.WeakDependencyProperty.TryGetTarget(out dependencyProperty);
                if (ReferenceEquals(dependencyProperty, null))
                {
                    return false;
                }

                dependencyProperty.ReplacePointer(cPtr);
            }

            dependencyProperty = null;
            return false;
        }

        public static void TryResurrectAll(Type ownerType)
        {
            var weakTypeEntry = new WeakTypeEntry(ownerType);
            if (!Dictionary.TryGetValue(weakTypeEntry, out var properties))
            {
                return;
            }

            foreach (var propertyData in properties)
            {
                var weakPropertyTypeEntry = propertyData.WeakPropertyTypeEntry;
                var propertyType = weakPropertyTypeEntry.Type;
                if (propertyType is null)
                {
                    continue;
                }

                var propertyMetadata = propertyData.PropertyMetadata.CreateFrameworkPropertyMetadata();
                DependencyProperty.Register(propertyData.Name,
                                            propertyType,
                                            ownerType,
                                            propertyMetadata);
            }
        }

        internal struct WeakTypeEntry : IEquatable<WeakTypeEntry>
        {
            private readonly int hashCode;

            private readonly WeakReference<Type> weakReference;

            public WeakTypeEntry(Type type)
            {
                this.weakReference = new WeakReference<Type>(type);
                this.hashCode = type.GetHashCode();
            }

            public Type Type
            {
                get
                {
                    return this.weakReference.TryGetTarget(out var type)
                               ? type
                               : null;
                }
            }

            public bool Equals(WeakTypeEntry other)
            {
                return this.hashCode == other.hashCode
                       && this.weakReference.TryGetTarget(out var typeA)
                       && other.weakReference.TryGetTarget(out var typeB)
                       && ReferenceEquals(typeA, typeB);
            }

            public override bool Equals(object obj)
            {
                return obj is WeakTypeEntry other
                       && this.Equals(other);
            }

            public override int GetHashCode()
            {
                return this.hashCode;
            }
        }

        private struct PropertyData
        {
            public WeakReference<DependencyProperty> WeakDependencyProperty;

            public PropertyData(
                string name,
                Type propertyType,
                PropertyMetadata propertyMetadata,
                DependencyProperty dependencyProperty)
            {
                this.Name = name;
                this.WeakPropertyTypeEntry = new WeakTypeEntry(propertyType);
                this.WeakDependencyProperty = new WeakReference<DependencyProperty>(dependencyProperty);
                this.PropertyMetadata = new StoredPropertyMetadataEntry(dependencyProperty, propertyMetadata);
            }

            public string Name { get; }

            public StoredPropertyMetadataEntry PropertyMetadata { get; }

            public WeakTypeEntry WeakPropertyTypeEntry { get; }
        }
    }
}