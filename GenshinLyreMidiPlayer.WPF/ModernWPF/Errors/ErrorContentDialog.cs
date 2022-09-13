using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using ModernWpf.Controls;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Errors;

public class ErrorContentDialog : ContentDialog
{
    public ErrorContentDialog(Exception e, IReadOnlyCollection<Enum>? options = null, string? closeText = null)
    {
        Title   = e.GetType().Name;
        Content = e.Message;

        PrimaryButtonText   = options?.ElementAtOrDefault(0)?.Humanize();
        SecondaryButtonText = options?.ElementAtOrDefault(1)?.Humanize();

        CloseButtonText = closeText ?? "Abort";
    }
}