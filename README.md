NoesisGUI MonoGame Integration
=============
This library provides solution for integration [NoesisGUI 3.2.9](http://noesisengine.com) with [MonoGame 3.8.4](http://monogame.net) library.
Currently it supports only MonoGame projects for Windows DirectX 11.
Example MonoGame project with integrated NoesisGUI is included.

Prerequisites
-----
* [Visual Studio 2022](https://www.visualstudio.com/), any edition (JetBrains Rider works fine too)

Installation
-----
1. Open `NoesisGUI.MonoGameWrapper.sln` with Visual Studio 2022.
2. Open context menu on `TestMonoGameNoesisGUI` project and select `Set as StartUp Project`.
3. Press F5 to launch the example game project.

Please note that the game example project uses default NoesisGUI theme from https://github.com/Noesis/Managed/tree/master/Src/NoesisApp/Theme and samples from https://github.com/Noesis/Tutorials/tree/master/Samples/Gallery/C%23 NoesisGUI works with XAML files without any preprocessing/building step (it has an extremely fast XAML parser that is faster than compiled BAML from WPF/UWP). You could store XAML files in any folder you want and robocopy them this way. One of the useful approaches is to store them in a WPF class library project which could be opened with Visual Studio to edit XAML with full syntax support and autocomplete. It also could be done as a WPF application project which could be executed independently to verify your UI is working properly (but that will require writing more demonstration logic there). WPF XAML is almost 100% compatible with NoesisGUI (see [docs](http://noesisengine.com/docs)).

Usage 
-----
To use NoesisGUI you must acquire a license. Once you have it, you need to pass it by calling `NoesisWrapper.Init(...)` method.
The included test project doesn't pass the license details so NoesisGUI operation is time-limited.

Implementation limitations
-----
* Currently only DirectX 11 is supported.
* Currently there are many PInvoke Windows dependencies for input handling (IME).

Contributing
-----
Pull requests are welcome.
Please make your code compliant with Microsoft recommended coding conventions:
* [General Naming Conventions](https://msdn.microsoft.com/en-us/library/ms229045%28v=vs.110%29.aspx) 
* [C# Coding Conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx)

License
-----
The code provided under MIT License. Please read [LICENSE.md](LICENSE.md) for details.
