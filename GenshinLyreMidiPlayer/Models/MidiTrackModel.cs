using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace GenshinLyreMidiPlayer.Models
{
    public class MidiTrackModel
    {
        public MidiTrackModel(TrackChunk track)
        {
            Track     = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
        }

        public bool IsChecked { get; set; }

        public string? TrackName { get; }

        public TrackChunk Track { get; }
    }
}