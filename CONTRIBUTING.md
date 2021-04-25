# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email (sabihoshi.dev@gmail.com), or any other method with me or the maintainers of this repository before making a change.

This project has a [Code of Conduct](CODE_OF_CONDUCT.md), please follow it in all your interactions with the project.

## Pull Request Process

1. Do not include the build itself where the project is cleaned using `dotnet clean`.
2. Update the README.md with details of changes to the project, new features, and others that are applicable.
3. Increase the version number of the project and the README.md to the new version that this
   Pull Request would represent. The versioning scheme we use is [SemVer](http://semver.org/).
4. You may merge the Pull Request in once you have the the approval of the maintainers.

## Build
If you just want to run the program, there are precompiled releases that can be found in [here](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases).
### Requirements
* [Git](https://git-scm.com) for cloning the project
* [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0) SDK

#### Publish a single binary for Windows
```bat
git clone https://github.com/sabihoshi/GenshinLyreMidiPlayer.git
cd GenshinLyreMidiPlayer\GenshinLyreMidiPlayer

dotnet publish -r win-x86 -c Release -o bin\publish --self-contained false -p:PublishSingleFile=true
```
> For other runtimes, visit the [RID Catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) and change the runtime value.

#### Build the project (not necessary if you published)
```bat
git clone https://github.com/sabihoshi/GenshinLyreMidiPlayer.git
cd GenshinLyreMidiPlayer

dotnet build
```

#### Publish the project using defaults
```bat
git clone https://github.com/sabihoshi/GenshinLyreMidiPlayer.git
cd GenshinLyreMidiPlayer

dotnet publish
```

### Notes
If you want to build using the [.Net Core 3.1 SDK](https://dotnet.microsoft.com/download), you need to make a few changes to the project files.

#### Project SDK
Change the Project SDK of `GenshinLyreMidiPlayer.WPF.csproj` into `Microsoft.NET.Sdk.WindowsDesktop`.
```diff
- <Project Sdk="Microsoft.NET.Sdk">
+ <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
```

### Language Version
Change the `LangVersion` of both projects into `preview`.
```diff
- <LangVersion>latest</LangVersion>
+ <LangVersion>preview</LangVersion>
```