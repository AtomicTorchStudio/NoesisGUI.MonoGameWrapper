NoesisGUI MonoGame Integration
=============
This library provides solution for integration [NoesisGUI](http://noesisengine.com) with [MonoGame](http://monogame.net) library.
Currently it supports only MonoGame projects for Windows DX11. OpenGL support planned.
Example MonoGame project with integrated NoesisGUI is included.

Prerequisites
-----
* [Visual Studio 2013 or 2015](https://www.visualstudio.com/), any edition will be fine.
* [MonoGame 3.* for VisualStudio](http://monogame.net)

Installation
-----
1. Download C# API Windows SDK from [NoesisGUI Forums](http://www.noesisengine.com/forums/viewtopic.php?f=3&t=91).
2. Extract it to the folder `\NoesisGUI-SDK\`. The resulting directory tree should look like this:
```
NoesisGUI-SDK
  |--BuildTool
  |--GLUTWrapper
  |--IntegrationSample
  |--NoesisGUI
```
3. Open `NoesisGUI.MonoGameWrapper.sln` with Visual Studio 2013/2015
4. Open `NoesisGUI` project properties and add the post build action to copy native `Noesis.dll` to the output libs directory:
```
robocopy /XO /NP /NJH /NJS "$(ProjectDir)NoesisGUI" "$(SolutionDir)Libs\Output" /IF Noesis.dll
if %errorlevel% gtr 3 (exit %errorlevel%) else (cd .)
```
5. Open context menu on `TestMonoGameNoesisGUI` project and select `Set as StartUp Project`.
6. Press F5 to launch the example game project.
7. Please note that the game example project uses pre-built NoesisGUI controls from NoesisGUI SDK (it robocopy them from it on post-build). To create an actual game you should use NoesisGUI Build Tool to build .XAML to .NSB files.

Roadmap
-----
* Add proper input key-repeat handling (currenly some keys (delete, arrows, etc) not repeating properly when you hold them).
* Add OpenGL support.
* Add configuration for Anti-Aliasing mode and offscreen buffer size (requires changes at NoesisGUI SDK project: method `UIRenderer.Resize(int width, int height)`).
* Loading XAML's without pre-building to .NSB-files (planned at NoesisGUI v1.3).
* Add touch input support.

Contributing
-----
Pull requests are welcome.
Please make your code complaint with Microsoft recommended coding conventions:
* [General Naming Conventions](https://msdn.microsoft.com/en-us/library/ms229045%28v=vs.110%29.aspx) 
* [C# Coding Conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx)

License
-----
The code provided under MIT License. Please read [LICENSE.md](LICENSE.md) for details.