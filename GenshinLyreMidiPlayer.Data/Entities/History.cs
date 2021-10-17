using System;

namespace GenshinLyreMidiPlayer.Data.Entities
{
    public class History
    {
        public History() { }

        public History(string path) { Path = path; }

        public Guid Id { get; set; }

        public string Path { get; set; } = null!;
    }
}