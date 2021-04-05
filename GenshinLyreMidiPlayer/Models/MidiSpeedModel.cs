namespace GenshinLyreMidiPlayer.Models
{
    public class MidiSpeedModel
    {
        public MidiSpeedModel(string speedName, double speed)
        {
            SpeedName = speedName;
            Speed     = speed;
        }

        public double Speed { get; }

        public string SpeedName { get; }
    }
}