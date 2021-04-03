# Genshin Wingsong Lyre Midi Player

A MIDI to key player made using C# and WPF.

![GenshinLyreMidiPlayer_2021-04-03_09-40-43](https://user-images.githubusercontent.com/25006819/113464637-c3f02a80-9460-11eb-838c-3416df611754.png)

## How to install

1. Download the latest version found here. [Download](https://github.com/sabihoshi/GenshinLyreMidiPlayer/releases/latest)
2. Run the program, no installation is required.
3. Open a .mid file by pressing the open file button at the top left.
4. Enable the tracks that you want to be played back.
5. Press play.

## Features

* You can play multiple tracks at the same time.
* You can enable transpose to transpose notes, otherwise it will skip the notes entirely.
* Written in C# WPF with a modern WinUI design.

## Upcoming

* The ability to change the key. It currently is in C major
* Play using your own MIDI Input Device
  - If you have your own MIDI instrument, this will let you play directly to the Genshin Lyre

## About

### What are MIDI files?
MIDI files (.mid) is a set of instructions that play various instruments on what are called tracks. You can enable specific tracks that you want it to play. It converts the notes on the track into the keyboard inputs in Genshin Impact. Currently it is tuned to C major.

### Can this get me banned?
The short answer is that it's uncertain. I have used this in development with my own account, and so far I have not gotten banned. But use it at your own rish. Do not play songs that will spam the keyboard, listen to the MIDI file first and make sure to play only one instrument so that the tool doesn't spam keyboard inputs.

## Special Thanks
* Credits to [ShawzinBot](https://github.com/ianespana/ShawzinBot) by [ianespana](ianespana) where most of the inspiration comes from.
* Lantua for explaining to me music theory; what octaves, transposition, keys, and scales are.
