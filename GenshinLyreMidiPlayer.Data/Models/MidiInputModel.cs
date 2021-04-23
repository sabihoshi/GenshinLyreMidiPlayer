namespace GenshinLyreMidiPlayer.Data.Models
{
    public class MidiInputModel
    {
        public MidiInputModel(string deviceName)
        {
            DeviceName = deviceName;
        }

        public string DeviceName { get; }
    }
}