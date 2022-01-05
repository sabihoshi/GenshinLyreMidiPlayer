namespace GenshinLyreMidiPlayer.Data.Midi;

public class MidiSpeed
{
    public MidiSpeed(string speedName, double speed)
    {
        SpeedName = speedName;
        Speed     = speed;
    }

    public double Speed { get; }

    public string SpeedName { get; }
}