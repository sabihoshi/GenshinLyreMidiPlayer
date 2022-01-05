using System.Linq;
using Melanchall.DryWetMidi.Core;
using Stylet;

namespace GenshinLyreMidiPlayer.Data.Midi;

public class MidiTrack
{
    private readonly IEventAggregator _events;
    private bool _isChecked;

    public MidiTrack(IEventAggregator events, TrackChunk track)
    {
        _events    = events;
        _isChecked = true;

        Track     = track;
        TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
    }

    public bool CanBePlayed => Track.Events.Count(e => e is NoteEvent) > 0;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            _isChecked = value;
            _events.Publish(this);
        }
    }

    public string? TrackName { get; }

    public TrackChunk Track { get; }
}