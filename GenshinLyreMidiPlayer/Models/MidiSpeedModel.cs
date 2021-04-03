namespace GenshinLyreMidiPlayer.Models
{
    public class MidiSpeedModel
    {
        public MidiSpeedModel(string speedName, double speed)
        {
            SpeedName = speedName;
            Speed = speed;
        }

        public string SpeedName { get; }

        public double Speed { get; }
    }
}