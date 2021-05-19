# 【 Genshin 原神 Windsong Lyre MIDI Auto Player 】

A MIDI to key player for Genshin Impact's Windsong Lyre made using C# and WPF using Windows Fluent design. If you enjoyed this project, consider [contributing](#contributing) or 🌟 starring the repository. Thank you~

> Check out [yoroshikun](https://github.com/yoroshikun)'s Flutter implementation of a **[Genshin Lyre Player](https://github.com/yoroshikun/flutter_genshin_lyre_player)** written in Dart!

## **[Download latest version][latest]** [![GitHub all releases](https://img.shields.io/github/downloads/sabihoshi/GenshinLyreMidiPlayer/total?style=social)][latest] [![GitHub release (latest by date)](https://img.shields.io/github/v/release/sabihoshi/GenshinLyreMidiPlayer)][latest]

![2021-04-13_22-51-12](https://user-images.githubusercontent.com/25006819/114573455-f8f34d00-9caa-11eb-8288-57c193ca2d04.png)

## How to use

1. [Download the program][latest] and then run, no need for installation.
2. Open a .mid file by pressing the open file button at the top left.
3. Enable the tracks that you want to be played back.
4. Press play it should automatically switch to Genshin Impact.
5. Automatically stops playing if you switch to a different window.

> If you get a [SmartScreen](https://user-images.githubusercontent.com/25006819/115977864-555d4300-a5ae-11eb-948b-c0139f606a2d.png) popup, click on "More info" and then "Run anyway"
> The reason this appears is because the application is not signed. Signing costs money which can get very expensive.

## Features
![GenshinLyreMidiPlayer WPF_2021-05-14_19-33-05](https://user-images.githubusercontent.com/25006819/118265668-1acf3200-b4ec-11eb-8b88-ded036563e18.png)
* The ability to change the key. By default, it is keyed to C major.
* You can play multiple tracks of a MIDI file at the same time.
* You can enable transposing of notes, otherwise it will skip the notes entirely.
* Written in C# WPF with modern fluent design.
* Change the keyboard layout (QWERTZ, AZERTY, DVORAK, etc.)
* You can test out MIDI files by playing through the speakers.
* History and Settings are now persisted even if you close the app.
* [![](https://img.shields.io/badge/v1.10.0-New!-yellow)](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/tag/v1.10.0) You can have the lyre auto play at a specified time. This can be found inside the settings page.
* [![](https://img.shields.io/badge/v1.10.2-New!-yellow)](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/tag/v1.10.2) Filter tracks using the search box.

### Media Controls
You can now control the Lyre natively by using your media controls that some keyboards have as special function keys. This integrates with other music applications as well.

![2021-05-04_13-25-52](https://user-images.githubusercontent.com/25006819/116963753-5132d300-acdc-11eb-85f8-e455928f8369.png)

### Play using your own MIDI Input Device
If you have your own MIDI instrument, this will let you play directly to the Genshin Lyre. This lets you play directly without using a MIDI file.

### Playlist Controls & History
A playlist allows you to play songs continuously without having to open a new file after a song has finished.

![GenshinLyreMidiPlayer_2021-04-13_22-27-05](https://user-images.githubusercontent.com/25006819/114570421-6651ae80-9ca8-11eb-9cb2-c9e322df14f0.png)

### Hold notes & Merge nearby notes
  - You can set the player to hold sustained notes (does not really make a difference. Off by default.)
  - Some songs sound better when nearby notes are merged see [#4](https://github.com/sabihoshi/GenshinLyreMidiPlayer/issues/4) for an example

### Light Mode
You can set the player to light mode/dark mode (uses your system's theme by default.)

![GenshinLyreMidiPlayer_2021-04-05_08-58-35](https://user-images.githubusercontent.com/25006819/113526575-237b4100-95ed-11eb-813c-1e9c661624cf.png)

### Mini Mode
You can resize the player as small as you want and it should close the panels accordingly.

![GenshinLyreMidiPlayer_2021-04-13_22-28-11](https://user-images.githubusercontent.com/25006819/114570320-4e7a2a80-9ca8-11eb-8907-a47025a0539a.png)

## Upcoming
* Output into a "Piano Sheet" in a text file.
* Drag and drop MIDI files into the player.
* Filter MIDI files by searching.

## About

### What are MIDI files?
MIDI files (.mid) is a set of instructions that play various instruments on what are called tracks. You can enable specific tracks that you want it to play. It converts the notes on the track into the keyboard inputs in Genshin Impact. Currently it is tuned to C major.

### Can this get me banned?
The short answer is that it's uncertain. I have used this in development with my own account for a week now and so far, I have not gotten banned. But use it at your own risk. Do not play songs that will spam the keyboard, listen to the MIDI file first and make sure to play only one instrument so that the tool doesn't spam keyboard inputs. [Here is miHoYo's response](https://genshin.mihoyo.com/en/news/detail/5763) to using 3rd party tools.

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
* [.NET 5.0](https://dotnet.microsoft.com/download) SDK

#### Publish a single binary for Windows
```bat
git clone https://github.com/sabihoshi/GenshinLyreMidiPlayer.git
cd GenshinLyreMidiPlayer

dotnet publish GenshinLyreMidiPlayer.WPF -r win-x64 ^
               -c Release --self-contained false -p:PublishSingleFile=true
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

#### Target Framework
Change the `TargetFramework` of both projects into `netcoreapp3.1`.
```diff
- <TargetFramework>net5.0-windows10.0.19041.0</TargetFramework>
+ <TargetFramework>netcoreapp3.1</TargetFramework>
```

#### Project SDK
Change the Project SDK of `GenshinLyreMidiPlayer.WPF.csproj` into `Microsoft.NET.Sdk.WindowsDesktop`.
```diff
- <Project Sdk="Microsoft.NET.Sdk">
+ <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
```

#### Language Version
Change the `LangVersion` of both projects into `preview`.
```diff
- <LangVersion>latest</LangVersion>
+ <LangVersion>preview</LangVersion>
```

# Special Thanks
* Credits to [ianespana](https://github.com/ianespana) and their project **[ShawzinBot](https://github.com/ianespana/ShawzinBot)** where most of the inspiration comes from.
* Credits to [yoroshikun](https://github.com/yoroshikun) and their project **[Flutter Genshin Lyre Auto Player](https://github.com/yoroshikun/flutter_genshin_lyre_player)** for giving ideas such as history and fluent design.
* **[Lantua](https://github.com/lantua)** for explaining to me music theory; what octaves, transposition, keys, and scales are.

# License
* This project is under the [MIT](LICENSE.md) license.
* All rights reserved by © miHoYo Co., Ltd. This project is not affiliated nor endorsed by miHoYo. Genshin Impact™ and other properties belong to their respective owners.
* This project uses third-party libraries or other resources that may be
distributed under [different licenses](THIRD-PARTY-NOTICES.md).

[latest]: https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/latest