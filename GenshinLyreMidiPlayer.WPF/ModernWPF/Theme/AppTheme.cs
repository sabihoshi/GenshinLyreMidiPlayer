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

    public static AppTheme Dark { get; } = new("黑暗", ApplicationTheme.Dark);

    public static AppTheme Default { get; } = new("跟随系统设置", null);

    public static AppTheme Light { get; } = new("明亮", ApplicationTheme.Light);

    public string Name { get; }

    public override string ToString() => Name;
}