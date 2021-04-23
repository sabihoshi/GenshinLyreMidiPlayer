using System.Linq;
using Melanchall.DryWetMidi.Core;
using Stylet;

namespace GenshinLyreMidiPlayer.Data.Models
{
    public class MidiTrackModel
    {
        private readonly IEventAggregator _events;
        private bool _isChecked;

        public MidiTrackModel(IEventAggregator events, TrackChunk track)
        {
            _events    = events;
            _isChecked = true;

            Track     = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                _events.Publish(this);
            }
        }

        public bool CanBePlayed => Track.Events.Count(e => e is NoteEvent) > 0;

        public string? TrackName { get; }

        public TrackChunk Track { get; }
    }
}