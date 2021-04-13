using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Stylet;
using static System.IO.Path;

namespace GenshinLyreMidiPlayer.Models
{
    public class MidiFileModel : Screen
    {
        private int _position;

        public MidiFileModel(string path, ReadingSettings? settings = null)
        {
            Path = path;
            Midi = MidiFile.Read(path, settings);
        }

        public int Position
        {
            get => _position + 1;
            set => SetAndNotify(ref _position, value);
        }

        public MidiFile Midi { get; }

        private string Path { get; }

        public string Title => GetFileNameWithoutExtension(Path);

        public TimeSpan Duration => Midi.GetDuration<MetricTimeSpan>();
    }
}