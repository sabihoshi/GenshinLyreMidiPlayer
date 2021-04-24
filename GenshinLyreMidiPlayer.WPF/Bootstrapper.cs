using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
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
        protected override void OnStart()
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
            builder.Bind<LyreContext>().ToFactory(_ =>
            {
                var config = ConfigurationManager
                    .OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

                var path = Path.GetDirectoryName(config.FilePath);
                var source = Path.Combine(path!, Settings.Default.SqliteConnection);

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var options = new DbContextOptionsBuilder<LyreContext>()
                    .UseSqlite($"Data Source={source}")
                    .Options;

                var db = new LyreContext(options);
                db.Database.EnsureCreated();

                return db;
            });
        }
    }
}