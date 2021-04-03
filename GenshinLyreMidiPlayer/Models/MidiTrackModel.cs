using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace GenshinLyreMidiPlayer.Models
{
    public class MidiTrackModel
    {
        public MidiTrackModel(TrackChunk track)
        {
            Track = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
        }

        public MidiTrackModel(TrackChunk track, bool isChecked)
        {
            Track = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
            IsChecked = isChecked;
        }

        public string TrackName { get; set; }

        public TrackChunk Track { get; set; }

        public bool IsChecked { get; set; }
    }
}