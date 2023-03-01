# 【 Genshin 原神 Windsong Lyre MIDI Auto Player 】

适用于原神的风物之诗琴、镜花之琴 和 老旧的诗琴 的关键播放器的 MIDI，使用 C# 和 WPF 使用 Windows Mica 设计制作。 如果您喜欢这个项目，请考虑 [贡献](#contributing) 或 🌟 为存储库加注星标。 谢谢~

## **[ 下载最新版本 ][latest]** [![GitHub all releases](https://img.shields.io/github/downloads/sabihoshi/GenshinLyreMidiPlayer/total?style=social)][latest] [![GitHub release (latest by date)](https://img.shields.io/github/v/release/sabihoshi/GenshinLyreMidiPlayer)][latest]

![2022-09-14_04-24-19](https://user-images.githubusercontent.com/25006819/190002173-fa8e2b0d-8817-4980-81f1-fb491c584310.png)

## 使用方法

1. [下载程序] [latest]，然后运行，不需要安装。
2. 通过按左上方的“打开文件”按钮打开一个.mid文件。
3. 启用您想播放的曲目。
4. 按Play播放它应自动切换到原神窗口。
5. 如果切换到其他窗口，则会自动停止播放。

> 如果你得到 [SmartScreen](https://user-images.githubusercontent.com/25006819/115977864-555d4300-a5ae-11eb-948b-c0139f606a2d.png) 弹出窗口，单击“更多信息”，然后“无视风险运行"
> 出现的原因是因为该应用程序未数字签名。 数字签名的成本可能会出现昂贵的开销。

## 特征
![GenshinLyreMidiPlayer WPF_2022-09-14_04-20-22](https://user-images.githubusercontent.com/25006819/190002309-ba67a17d-6b3f-4239-ba73-1e0e05b9ad1b.png)
* 可以更改键值. 默认为C大调。
* 您可以同时播放MIDI文件的多个曲目。
* 您可以启用记录的转载，否则它将完全跳过记录。
* 用W11云母设计用C＃WPF编写。
* 更改键盘布局 (QWERTZ, AZERTY, DVORAK, etc.)
* 您可以通过扬声器播放来测试MIDI文件。
* 即使您关闭了应用程序，历史和设置也会持续存在。
* 您可以在指定的时间进行风物之诗琴自动播放。 可以在“设置”页面中找到。
* [![](https://img.shields.io/badge/v1.10.2-New!-yellow)](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/tag/v1.10.2) 支持关键字搜索曲目

# Piano Sheet [![](https://img.shields.io/badge/v2.1.0.1-New!-yellow)](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/tag/v2.1.0.1)
已经添加了钢琴表的第一个版本，这使您可以轻松地与他人分享歌曲，或者让自己尝试。 您可以更改定界符以及拆分尺寸和间距。 这将使用您选择的当前键盘布局。

![GenshinLyreMidiPlayer WPF_2022-09-14_04-27-50](https://user-images.githubusercontent.com/25006819/190002764-b5a74b2c-2402-462f-b35e-70ab4c45b5ec.gif)

### Media Controls
现在，您可以通过使用一些键盘作为特殊功能键的媒体控件来本地控制乐器。 这也与其他音乐应用程序集成在一起。

![2021-05-04_13-25-52](https://user-images.githubusercontent.com/25006819/116963753-5132d300-acdc-11eb-85f8-e455928f8369.png)

### 使用自己的MIDI输入设备播放
如果您有自己的MIDI乐器，这将使您直接输出到风物之诗琴。 这使您无需使用MIDI文件即可直接播放。

### 播放列表控件和历史
播放列表允许您连续播放歌曲，而无需在歌曲完成后打开新文件。

![2022-09-14_04-28-38](https://user-images.githubusercontent.com/25006819/190002884-17fc502c-5235-42c9-9283-3703328a63ea.png)

### 开启记录并合并附近的记录
- 您可以将播放器设置为保持持续的记录（默认情况下不发挥作用。）
- 当附近的音符合并时，有些歌曲听起来更好 [#4](https://github.com/sabihoshi/GenshinLyreMidiPlayer/issues/4) 示例

### 明亮模式
您可以将播放器设置为明亮模式/暗模式（默认使用系统主题。）

![GenshinLyreMidiPlayer WPF_2022-09-14_04-29-00](https://user-images.githubusercontent.com/25006819/190002946-e43658c1-cf3a-469d-9ab5-a166f34f673c.png)

### 迷你模式
您可以根据需要调整播放器的大小，并且应该相应关闭面板。

![GenshinLyreMidiPlayer WPF_2022-09-14_04-21-55](https://user-images.githubusercontent.com/25006819/190002986-aabb84df-924f-48fc-9354-573635dc2551.png)

## 即将到来
* 将MIDI文件拖放到播放器中。

## 关于

### 什么是Midi文件？
MIDI文件（.mid）是一组指令，可以在所谓的轨道上播放各种乐器。 您可以启用您希望它播放的特定曲目。 它将轨道上的音符转换为原神中的键盘输入。 目前，它已调整为C大调。

### 会导致封禁吗？
简短的答案是它不确定。 我已经在自己的帐户中使用了这一点，已经有一个星期了，到目前为止，我还没有被禁止。 但是使用它自身的风险。 不要播放会垃圾键盘垃圾邮件的歌曲，首先收听MIDI文件，然后确保仅弹奏一个乐器，以免工具垃圾邮件输入。 [这是Mihoyo的回应]（https://genshin.mihoyo.com/en/news/detail/5763）使用第三方工具。
# 贡献
在为此存储库做出贡献时，请首先讨论您希望通过发行的更改，电子邮件（sabihoshi.dev@gmail.com）或与我或该存储库的维护者进行更改之前，或与我或该存储库的维护者进行更改。

这个项目有一个 [Code of Conduct](CODE_OF_CONDUCT.md), 请在您与项目的所有互动中遵循它。

## 拉取请求

1. 不要包括使用项目清洁项目的构建本身 `dotnet clean`.
2. 更新readme.md，其中包含有关项目更改，新功能以及其他适用其他功能的详细信息。
3. 将项目的版本编号和readme.md增加到新版本
   拉请求代表。 我们使用的版本控制方案是 [SemVer](http://semver.org/).
4. 一旦获得维护人员的批准，您就可以合并拉动请求。

## 构建
如果您只想运行该程序，则可以在[here](https://github.com/sabihoshi/genshinlyremidiplayer/releases)中找到预编译版本。
### 要求
* [Git](https://git-scm.com) for cloning the project
* [.NET 6.0](https://dotnet.microsoft.com/download) SDK

#### 为Windows发布单个二进制文件
```bat
git clone https://github.com/sabihoshi/GenshinLyreMidiPlayer.git
cd GenshinLyreMidiPlayer

dotnet publish GenshinLyreMidiPlayer.WPF -r win-x64 ^
               -c Release --self-contained false -p:PublishSingleFile=true
```
> 对于其他运行时间，请访问[RID目录](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)并更改运行时值。

#### 构建项目（如果您发布的话，则不必要）
```bat
git clone https://github.com/sabihoshi/GenshinLyreMidiPlayer.git
cd GenshinLyreMidiPlayer

dotnet build
```

#### 使用默认项目发布项目
```bat
git clone https://github.com/sabihoshi/GenshinLyreMidiPlayer.git
cd GenshinLyreMidiPlayer

dotnet publish
```

### Notes
如果要使用 [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)构建，则需要对项目文件进行一些更改。
#### 目标框架
将两个项目的`targetFrameWork`  更改为 `netCoreApp3.1`。
```diff
- <TargetFramework>net6.0-windows10.0.22621.0</TargetFramework>
+ <TargetFramework>netcoreapp3.1</TargetFramework>
```

#### 项目 SDK
将此项目的SDK `GenshinLyreMidiPlayer.WPF.csproj` 更改为`Microsoft.NET.Sdk.WindowsDesktop`.
```diff
- <Project Sdk="Microsoft.NET.Sdk">
+ <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
```

#### 语言版本
将两个项目的`langversion`更改为`preview`。
```diff
- <LangVersion>latest</LangVersion>
+ <LangVersion>preview</LangVersion>
```

# 特别鸣谢
* Credits to [ianespana](https://github.com/ianespana) and their project **[ShawzinBot](https://github.com/ianespana/ShawzinBot)** where most of the inspiration comes from.
* Credits to [yoroshikun](https://github.com/yoroshikun) and their project **[Flutter Genshin Lyre Auto Player](https://github.com/yoroshikun/flutter_genshin_lyre_player)** for giving ideas such as history and fluent design.
* **[Lantua](https://github.com/lantua)** for explaining to me music theory; what octaves, transposition, keys, and scales are.

# 许可证
* 该项目遵循 [MIT](LICENSE.md) 许可证.
* ©Mihoyo Co.，Ltd保留的所有权利。该项目不隶属于Mihoyo。 Genshin Impact™和其他属性属于其各自的所有者。
* 该项目使用第三方库或其他可能是的资源
  分布在 [different licenses](THIRD-PARTY-NOTICES.md).

[latest]: https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/latest

