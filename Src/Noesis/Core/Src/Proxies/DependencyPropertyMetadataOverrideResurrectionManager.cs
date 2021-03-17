namespace Noesis
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using static DependencyPropertyResurrectionManager;

    /// <summary>
    /// This class is used for resurrection of the default value override (such as for default style override).
    /// </summary>
    public class DependencyPropertyMetadataOverrideResurrectionManager
    {
        private static readonly Dictionary<WeakTypeEntry, List<StoredPropertyMetadataEntry>> Dictionary
            = new Dictionary<WeakTypeEntry, List<StoredPropertyMetadataEntry>>();

        public static void Register(
            DependencyProperty dependencyProperty,
            Type forType,
            PropertyMetadata typeMetadata)
        {
            var weakTypeEntry = new WeakTypeEntry(forType);
            if (!Dictionary.TryGetValue(weakTypeEntry, out var listMetadata))
            {
                listMetadata = new List<StoredPropertyMetadataEntry>();
                Dictionary[weakTypeEntry] = listMetadata;
            }

            listMetadata.Add(new StoredPropertyMetadataEntry(dependencyProperty, typeMetadata));
        }

        public static void TryResurrect(Type forType)
        {
            var weakTypeEntry = new WeakTypeEntry(forType);
            if (!Dictionary.TryGetValue(weakTypeEntry, out var listMetadata))
            {
                return;
            }

            // remove the reference as new overrides will be created during the following OverrideMetadata calls 
            Dictionary.Remove(weakTypeEntry);

            foreach (var entry in listMetadata)
            {
                var dependencyPropertyName = entry.DependencyPropertyName + "Property";
                var property = forType.GetProperty(dependencyPropertyName,
                                                   BindingFlags.FlattenHierarchy
                                                   | BindingFlags.Static
                                                   | BindingFlags.Public
                                                   | BindingFlags.NonPublic);
                if (property is null)
                {
                    Log.Error("Dependency property not found: "
                              + dependencyPropertyName
                              + " in "
                              + forType.FullName);
                    continue;
                }

                var dependencyProperty = property.GetValue(null) as DependencyProperty;
                if (dependencyProperty is null)
                {
                    Log.Error("The dependency property doesn't exist");
                    continue;
                }

                if (dependencyProperty.IsDisposed)
                {
                    Log.Error("The dependency property is disposed");
                    continue;
                }

                var metadata = entry.CreateFrameworkPropertyMetadata();
                dependencyProperty.OverrideMetadata(forType, metadata);
            }
        }
    }
}