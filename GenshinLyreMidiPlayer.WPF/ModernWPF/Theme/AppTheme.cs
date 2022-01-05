using ModernWpf;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Theme;

public class AppTheme
{
    private AppTheme(string name, ApplicationTheme? value)
    {
        Name  = name;
        Value = value;
    }

    public ApplicationTheme? Value { get; }

    public static AppTheme Dark { get; } = new("Dark", ApplicationTheme.Dark);

    public static AppTheme Default { get; } = new("Use system setting", null);

    public static AppTheme Light { get; } = new("Light", ApplicationTheme.Light);

    public string Name { get; }

    public override string ToString() => Name;
}