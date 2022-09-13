using System;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Errors;

public class MissingNotesException : Exception
{
    public MissingNotesException(string message) : base(message) { }
}