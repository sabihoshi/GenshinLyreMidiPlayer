using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using ModernWpf.Controls;

namespace GenshinLyreMidiPlayer.WPF.Core.Errors
{
    public class ErrorContentDialog : ContentDialog
    {
        public ErrorContentDialog(Exception e, IReadOnlyCollection<Enum> options = null)
        {
            Title   = e.Message;
            Content = e;

            PrimaryButtonText   = options?.ElementAtOrDefault(0)?.Humanize();
            SecondaryButtonText = options?.ElementAtOrDefault(1)?.Humanize();

            CloseButtonText = "Abort";
        }
    }
}