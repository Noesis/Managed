NoesisGUI Managed SDK
=====================

This repository contains source code for [NuGet packages](https://www.nuget.org/profiles/NoesisTechnologies) corresponding to the C# SDK of [NoesisGUI](https://www.noesisengine.com).

Organization
------------

Packages are subdivided into two big categories: **Noesis** the Core library and **NoesisApp** the application framework used by our [samples](https://github.com/Noesis/Tutorials). The Visual Studio 2017 root solution 'Noesis.sln' contains all the projects for all supported platforms.

* Noesis
  - Core Library
  - Extensions
* NoesisApp
  - Core Framework
  - Displays: Win32, WinRT, X11, UIKit, AppKit, Android...
  - RenderContexts: D3D11, GLX, EGL, NSGL, WGL, MTL...

Supported platforms
-------------------

* Windows (x86, x64)
* UWP (x86, x64, arm)
* macOS (x64)
* Linux (x64)
* iOS (arm, arm64)
* Android (arm, arm64, x86)

