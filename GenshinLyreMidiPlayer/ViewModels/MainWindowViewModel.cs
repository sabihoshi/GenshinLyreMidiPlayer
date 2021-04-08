using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using GenshinLyreMidiPlayer.Models;
using GenshinLyreMidiPlayer.Views;
using ModernWpf.Controls;
using Stylet;

namespace GenshinLyreMidiPlayer.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.StackNavigation
    {
        private readonly Stack<NavigationViewItem> _history = new();

        public MainWindowViewModel(IEventAggregator events)
        {
            SettingsView = new SettingsPageViewModel(events);
            PlayerView   = new LyrePlayerViewModel(events, SettingsView);

            Task.Run(async () =>
            {
                UpdateString     += "(Checking for updates)";
                IsCheckingUpdate =  true;

                try
                {
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get,
                        "https://api.github.com/repos/sabihoshi/GenshinLyreMidiPlayer/releases/latest");

                    var productInfo = new ProductInfoHeaderValue("GenshinLyreMidiPlayer", ProgramVersion?.ToString());

                    request.Headers.UserAgent.Add(productInfo);

                    var response = await client.SendAsync(request);
                    var version = JsonSerializer.Deserialize<GitVersion>(await response.Content.ReadAsStringAsync());
                    if (version.Version > ProgramVersion && !(version.Draft || version.Prerelease))
                        UpdateString = $"(Update available! {version.TagName})";
                    else
                        UpdateString = string.Empty;
                }
                catch (Exception)
                {
                    UpdateString = "(Failed to check updates)";
                }
                finally
                {
                    IsCheckingUpdate = false;
                }
            });
        }

        public bool IsCheckingUpdate { get; set; }

        public LyrePlayerViewModel PlayerView { get; }

        public SettingsPageViewModel SettingsView { get; }

        public string Title { get; set; } = "Genshin Lyre MIDI Player";

        public string VersionString { get; set; } = $"v{ProgramVersion?.ToString(3)}";

        public string UpdateString { get; set; } = string.Empty;

        private static Version? ProgramVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        protected override void OnViewLoaded()
        {
            // Work around because events do not conform to the signatures Stylet supports
            var navView = ((MainWindowView) View).NavView;
            navView.SelectionChanged += Navigate;
            navView.BackRequested    += NavigateBack;

            var menuItems = navView.MenuItems.Cast<NavigationViewItemBase>();
            navView.SelectedItem = menuItems.FirstOrDefault(item => item is NavigationViewItem);
        }

        private void NavigateBack(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            GoBack();

            // Work around to select the navigation item that this IScreen is a part of
            _history.Pop();
            sender.IsBackEnabled = _history.Count > 2;
            sender.SelectedItem  = _history.Pop();
        }

        private void Navigate(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ActivateItem(SettingsView);
                _history.Push((NavigationViewItem) sender.SettingsItem);
            }
            else if ((args.SelectedItem as NavigationViewItem)?.Tag is IScreen viewModel)
            {
                ActivateItem(viewModel);
                _history.Push((NavigationViewItem) sender.SelectedItem);
            }

            sender.IsBackEnabled = _history.Count > 1;
        }
    }
}