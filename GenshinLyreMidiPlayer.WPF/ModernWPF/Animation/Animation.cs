using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Animation;

public class Animation
{
    private readonly FrameworkElement _element;
    private readonly Storyboard _storyboard;
    private ClockState _currentState = ClockState.Stopped;

    static Animation()
    {
        var defaultBitmapCache = new BitmapCache();
        defaultBitmapCache.Freeze();
    }

    public Animation(FrameworkElement element, Storyboard storyboard)
    {
        _element                            =  element;
        _storyboard                         =  storyboard;
        _storyboard.CurrentStateInvalidated += OnCurrentStateInvalidated;
        _storyboard.Completed               += OnCompleted;
    }

    public event EventHandler? Completed;

    public void Begin()
    {
        if (!(_element.CacheMode is BitmapCache))
            _element.SetCurrentValue(UIElement.CacheModeProperty, GetBitmapCache());
        _storyboard.Begin(_element, true);
    }

    public void Stop()
    {
        if (_currentState != ClockState.Stopped) _storyboard.Stop(_element);
        _element.InvalidateProperty(UIElement.CacheModeProperty);
        _element.InvalidateProperty(UIElement.RenderTransformProperty);
        _element.InvalidateProperty(UIElement.RenderTransformOriginProperty);
    }

    private BitmapCache GetBitmapCache()
    {
#if NETCOREAPP || NET462
        return new(VisualTreeHelper.GetDpi(_element).PixelsPerDip);
#else
            return _defaultBitmapCache;
#endif
    }

    private void OnCompleted(object? sender, EventArgs e) => Completed?.Invoke(this, EventArgs.Empty);

    private void OnCurrentStateInvalidated(object? sender, EventArgs e)
    {
        if (sender is Clock clock) _currentState = clock.CurrentState;
    }
}