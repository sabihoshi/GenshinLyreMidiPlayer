using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using GenshinLyreMidiPlayer.Data;
using GenshinLyreMidiPlayer.Data.Properties;
using GenshinLyreMidiPlayer.WPF.ViewModels;
using Microsoft.EntityFrameworkCore;
using Stylet;
using StyletIoC;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace GenshinLyreMidiPlayer.WPF;

public class Bootstrapper : Bootstrapper<MainWindowViewModel>
{
    public Bootstrapper()
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

    protected override void ConfigureIoC(IStyletIoCBuilder builder)
    {
        var config = ConfigurationManager
            .OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

        var path = Path.GetDirectoryName(config.FilePath);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path!);

        builder.Bind<LyreContext>().ToFactory(_ =>
        {
            var source = Path.Combine(path!, Settings.Default.SqliteConnection);

            var options = new DbContextOptionsBuilder<LyreContext>()
                .UseSqlite($"Data Source={source}")
                .Options;

            var db = new LyreContext(options);
            db.Database.EnsureCreated();

            return db;
        });

        builder.Bind<MediaPlayer>().ToFactory(_ =>
        {
            var player = new MediaPlayer();
            var controls = player.SystemMediaTransportControls;

            controls.IsEnabled           = true;
            controls.DisplayUpdater.Type = MediaPlaybackType.Music;

            Task.Run(async () =>
            {
                const string name = "item_windsong_lyre.png";
                var location = Path.Combine(path!, name);

                var uri = new Uri($"pack://application:,,,/{name}");
                var resource = Application.GetResourceStream(uri)!.Stream;
                Image.FromStream(resource).Save(location);

                var file = await StorageFile.GetFileFromPathAsync(location);
                controls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromFile(file);
            });

            return player;
        }).InSingletonScope();

        builder.Bind<IThemeService>().To<ThemeService>().InSingletonScope();
    }
}