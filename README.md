NoesisGUI MonoGame Integration
=============
This library provides solution for integration [NoesisGUI 2.0](http://noesisengine.com) with [MonoGame 3.6](http://monogame.net) library.
Currently it supports only MonoGame projects for Windows DirectX 11. OpenGL support planned.
Example MonoGame project with integrated NoesisGUI is included.

Prerequisites
-----
* [Visual Studio 2013/2015/2017](https://www.visualstudio.com/), any edition.
* [MonoGame 3.6 for VisualStudio](http://monogame.net).
* [NoesisGUI 2.0 Managed SDK (C#)](http://www.noesisengine.com/developers/downloads.php)

Installation
-----
1. Download [NoesisGUI 2.0 Managed SDK (C#)](http://www.noesisengine.com/developers/downloads.php).
2. Extract it to the folder `\NoesisGUI-SDK\`. The resulting directory tree should look like this:
        
        NoesisGUI-CSharpSDK
          |--Bin
          |--Data
          |--Doc
          |--Lib
          |--Src
        
3. Open `NoesisGUI.MonoGameWrapper.sln` with Visual Studio 2013/2015/2017.
4. Open context menu on `TestMonoGameNoesisGUI` project and select `Set as StartUp Project`.
5. Press F5 to launch the example game project.
6. Please note that the game example project uses sample XAML files from NoesisGUI SDK (it robocopy them from it on post-build). You could store XAML files in any folder you want and robocopy them this way. One of the useful approaches is to store them in a WPF class library project which could be opened with Visual Studio to edit XAML with full syntax support and autocomplete. It also could be done as a WPF application project which could be executed independently to verify your UI is working properly (but that will require writing more demonstration logic here). WPF XAML is almost 100% compatible with NoesisGUI (see [docs](noesisengine.com/docs)).
7. Please note that the game example project uses copy `NoesisGUI-CSharpSDK\Bin\windows_x86\Noesis.dll` during the post-build event.

Roadmap
-----
* Add OpenGL support.
* Make it platform-independent (currently there are a few PInvoke Windows dependencies).
* Add touch input support.

Contributing
-----
Pull requests are welcome.
Please make your code compliant with Microsoft recommended coding conventions:
* [General Naming Conventions](https://msdn.microsoft.com/en-us/library/ms229045%28v=vs.110%29.aspx) 
* [C# Coding Conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx)

License
-----
The code provided under MIT License. Please read [LICENSE.md](LICENSE.md) for details.
