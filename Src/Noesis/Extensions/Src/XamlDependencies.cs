////////////////////////////////////////////////////////////////////////////////////////////////////
// Noesis Engine - http://www.noesisengine.com
// Copyright (c) 2009-2010 Noesis Technologies S.L. All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows;

/// <summary>
/// Helper classes to inject dependencies to XAMLs
/// </summary>
namespace NoesisGUIExtensions
{
    public class Dependency : DependencyObject
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(Uri), typeof(Dependency), new PropertyMetadata(null));

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
    }

    public class Xaml : DependencyObject
    {
        // Shadow property name so xaml parser does not skip the GetDependencies static method
        private static readonly DependencyProperty DependenciesProperty = DependencyProperty.RegisterAttached(
            "ShadowDependencies", typeof(List<Dependency>), typeof(Xaml), new PropertyMetadata(null));

        public static List<Dependency> GetDependencies(DependencyObject d)
        {
            if (d == null) throw new ArgumentNullException("d");

            List<Dependency> deps = (List<Dependency>)d.GetValue(DependenciesProperty);
            if (deps == null)
            {
                deps = new List<Dependency>();
                d.SetValue(DependenciesProperty, deps);
            }

            return deps;
        }
    }
}