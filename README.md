# Genshin Windsong Lyre Midi Player

A MIDI to key player made using C# and WPF using Windows Fluent design.

![2021-04-04_08-03-01](https://user-images.githubusercontent.com/25006819/113494611-61f4fb00-951c-11eb-98d2-c13980def63e.png)

## How to install

1. Download the latest version found here. [Download](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/latest)
2. Run the program, no installation is required.
3. Open a .mid file by pressing the open file button at the top left.
4. Enable the tracks that you want to be played back.
5. Press play.

## Features

* The ability to change the key. By default it is keyed to C major.
* You can play multiple tracks of a MIDI file at the same time.
* You can enable transposing of notes, otherwise it will skip the notes entirely.
* Written in C# WPF with modern fluent design.
* [![](https://img.shields.io/badge/v1.2.0-New!-yellow)](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/tag/v1.2.0) Play using your own MIDI Input Device
  - If you have your own MIDI instrument, this will let you play directly to the Genshin Lyre
* [![](https://img.shields.io/badge/v1.3.1-New!-yellow)](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/tag/v1.3.1) Change the keyboard layout (QWERTZ, AZERTY, DVORAK, etc.)

## Upcoming
* Output into a "Piano Sheet" in a text file.
* History of opened MIDI files.
* Light/Dark Theme overrides (currently respects your system theme.)

## About

### What are MIDI files?
MIDI files (.mid) is a set of instructions that play various instruments on what are called tracks. You can enable specific tracks that you want it to play. It converts the notes on the track into the keyboard inputs in Genshin Impact. Currently it is tuned to C major.

### Can this get me banned?
The short answer is that it's uncertain. I have used this in development with my own account, and so far I have not gotten banned. But use it at your own rish. Do not play songs that will spam the keyboard, listen to the MIDI file first and make sure to play only one instrument so that the tool doesn't spam keyboard inputs.

## Special Thanks
* Credits to [ShawzinBot](https://github.com/ianespana/ShawzinBot) by [ianespana](ianespana) where most of the inspiration comes from.
* Lantua for explaining to me music theory; what octaves, transposition, keys, and scales are.
