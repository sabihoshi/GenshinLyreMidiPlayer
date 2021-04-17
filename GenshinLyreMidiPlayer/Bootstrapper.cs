using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using GenshinLyreMidiPlayer.ViewModels;
using Stylet;

namespace GenshinLyreMidiPlayer
{
    public class Bootstrapper : Bootstrapper<MainWindowViewModel>
    {
        protected override void Configure()
        {
            // Make Hyperlinks handle themselves
            EventManager.RegisterClassHandler(
                typeof(Hyperlink), Hyperlink.RequestNavigateEvent,
                new RequestNavigateEventHandler((_, e) =>
                {
                    var url = e.Uri.ToString();
                    Process.Start(new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    });
                })
            );
        }
    }
}