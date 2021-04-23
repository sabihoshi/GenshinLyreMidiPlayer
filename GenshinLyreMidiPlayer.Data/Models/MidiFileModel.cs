using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Stylet;
using static System.IO.Path;

namespace GenshinLyreMidiPlayer.Data.Models
{
    public class MidiFileModel : Screen
    {
        private readonly ReadingSettings? _settings;
        private int _position;

        public MidiFileModel(string path, ReadingSettings? settings = null)
        {
            _settings = settings;

            Path = path;
        }

        public int Position
        {
            get => _position + 1;
            set => SetAndNotify(ref _position, value);
        }

        public MidiFile Midi => MidiFile.Read(Path, _settings);

        private string Path { get; }

        public string Title => GetFileNameWithoutExtension(Path);

        public TimeSpan Duration => Midi.GetDuration<MetricTimeSpan>();

        public event PropertyChangedEventHandler PropertyChanged;

        public MidiFile GetMidi() => MidiFile.Read(Path, _settings);

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}