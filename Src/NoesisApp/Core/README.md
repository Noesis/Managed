Source code corresponding to [Noesis.App](https://www.nuget.org/packages/Noesis.App)

Although noesisGUI can be fully integrated within your application, an **Application Framework** is also provided. This framework is a library that provides abstractions to create multiplatform applications. The framework is a good example about how to use Noesis and it provides many helpers that can be useful. All our public [samples](https://github.com/Noesis/Tutorials/tree/master/Samples) use the application framework.

The extensions provided, like *Application* and *Window*, are compatible with WPF, so compatible with Microsoft Blend. Using the application framework you can design applications that will indistinctly run in a Windows window, macOS window or iPhone screen for example. The framework supports all the platforms where NoesisGUI is available.

The following is a list of the functionality exposed by the Application Framework:

* Multiplatform abstractions for Application and Window.
* Multiplatform handling of input events: keyboard, mouse, touch and gamepads.
* Support for playing sounds.
* Resource Providers implementation for loading assets.
* Render implementations for each supported graphics API.
* The [Interactivity](https://www.noesisengine.com/docs/App.Interactivity.Behaviors.html) package.
