using GenshinLyreMidiPlayer.Data.Midi;

namespace GenshinLyreMidiPlayer.Data.Notification;

public class MergeNotesNotification
{
    public MergeNotesNotification(bool merge) { Merge = merge; }

    public bool Merge { get; }
}

public class TrackNotification
{
    public TrackNotification(MidiTrack track, bool enabled)
    {
        Track   = track;
        Enabled = enabled;
    }

    public bool Enabled { get; }

    public MidiTrack Track { get; }
}