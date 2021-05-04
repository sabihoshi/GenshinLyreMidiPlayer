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

namespace GenshinLyreMidiPlayer.WPF
{
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
            var level = ConfigurationUserLevel.PerUserRoamingAndLocal;
            var config = ConfigurationManager.OpenExeConfiguration(level);

            var path = Path.GetDirectoryName(config.FilePath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

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
                    var icon = Path.Combine(path!, "icon.png");
                    var uri = new Uri("pack://application:,,,/item_windsong_lyre.png");
                    var resource = Application.GetResourceStream(uri)!.Stream;
                    Image.FromStream(resource)
                        .Save(icon);

                    var file = await StorageFile.GetFileFromPathAsync(icon);
                    controls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromFile(file);
                });

                return player;
            });
        }
    }
}