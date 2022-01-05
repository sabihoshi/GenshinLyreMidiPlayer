using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Animation;

/// <summary>
///     Provides parameter info for the Frame.Navigate method. Controls how the transition
///     animation runs during the navigation action.
/// </summary>
public abstract class Transition : DependencyObject
{
    internal static readonly KeySpline AccelerateKeySpline;
    internal static readonly KeySpline DecelerateKeySpline;
    internal static readonly PropertyPath OpacityPath = new(UIElement.OpacityProperty);

    internal static readonly PropertyPath TranslateXPath =
        new("(UIElement.RenderTransform).(TranslateTransform.X)");

    internal static readonly PropertyPath TranslateYPath =
        new("(UIElement.RenderTransform).(TranslateTransform.Y)");

    internal static readonly PropertyPath ScaleXPath =
        new("(UIElement.RenderTransform).(ScaleTransform.ScaleX)");

    internal static readonly PropertyPath ScaleYPath =
        new("(UIElement.RenderTransform).(ScaleTransform.ScaleY)");

    internal static readonly TimeSpan ExitDuration = TimeSpan.FromMilliseconds(150);
    internal static readonly TimeSpan EnterDuration = TimeSpan.FromMilliseconds(300);
    internal static readonly TimeSpan MaxMoveDuration = TimeSpan.FromMilliseconds(500);

    static Transition()
    {
        AccelerateKeySpline = new(0.7, 0, 1, 0.5);
        AccelerateKeySpline.Freeze();

        DecelerateKeySpline = new(0.1, 0.9, 0.2, 1);
        DecelerateKeySpline.Freeze();
    }

    public Animation? GetEnterAnimation(object element, bool movingBackwards) =>
        GetEnterAnimation(
            (FrameworkElement) element, movingBackwards);

    public Animation? GetExitAnimation(object element, bool movingBackwards) =>
        GetExitAnimation(
            (FrameworkElement) element, movingBackwards);

    //protected virtual string GetNavigationStateCore();
    //protected virtual void SetNavigationStateCore(string navigationState);

    protected abstract Animation? GetEnterAnimation(FrameworkElement element, bool movingBackwards);

    protected abstract Animation? GetExitAnimation(FrameworkElement element, bool movingBackwards);
}