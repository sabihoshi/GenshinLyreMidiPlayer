using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using static System.IO.Path;

namespace GenshinLyreMidiPlayer.Models
{
    public class MidiFileModel
    {
        public MidiFileModel(string path, ReadingSettings? settings = null)
        {
            Path = path;
            Midi = MidiFile.Read(path, settings);
        }

        public MidiFile Midi { get; }

        private string Path { get; }

        public string Title => GetFileNameWithoutExtension(Path);

        public TimeSpan Duration => Midi.GetDuration<MetricTimeSpan>();
    }
}