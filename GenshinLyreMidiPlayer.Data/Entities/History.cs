using System;

namespace GenshinLyreMidiPlayer.Data.Entities;

public class History
{
    protected History() { }

    public History(string path, int key)
    {
        Key  = key;
        Path = path;
    }

    public Guid Id { get; set; }

    public int Key { get; set; }

    public string Path { get; set; } = null!;

    public Transpose? Transpose { get; set; }
}