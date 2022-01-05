using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Animation.Transitions;

/// <summary>
///     Specifies the animation to run when content appears on a Page.
/// </summary>
public sealed class EntranceTransition : Transition
{
    protected override Animation GetEnterAnimation(FrameworkElement element, bool movingBackwards)
    {
        var storyboard = new Storyboard();

        if (movingBackwards)
        {
            var opacityAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                    new SplineDoubleKeyFrame(1, EnterDuration, DecelerateKeySpline)
                }
            };
            Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
            storyboard.Children.Add(opacityAnim);
        }
        else
        {
            var yAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(200, TimeSpan.Zero),
                    new SplineDoubleKeyFrame(0, EnterDuration, DecelerateKeySpline)
                }
            };
            Storyboard.SetTargetProperty(yAnim, TranslateYPath);
            storyboard.Children.Add(yAnim);

            var opacityAnim = new DoubleAnimation(1, TimeSpan.Zero);
            Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
            storyboard.Children.Add(opacityAnim);

            element.SetCurrentValue(UIElement.RenderTransformProperty, new TranslateTransform());
        }

        return new(element, storyboard);
    }

    protected override Animation GetExitAnimation(FrameworkElement element, bool movingBackwards)
    {
        var storyboard = new Storyboard();

        if (movingBackwards)
        {
            var yAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                    new SplineDoubleKeyFrame(200, ExitDuration, AccelerateKeySpline)
                }
            };
            Storyboard.SetTargetProperty(yAnim, TranslateYPath);
            storyboard.Children.Add(yAnim);

            var opacityAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(1, TimeSpan.Zero),
                    new DiscreteDoubleKeyFrame(0, ExitDuration)
                }
            };
            Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
            storyboard.Children.Add(opacityAnim);

            element.SetCurrentValue(UIElement.RenderTransformProperty, new TranslateTransform());
        }
        else
        {
            var opacityAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(1, TimeSpan.Zero),
                    new SplineDoubleKeyFrame(0, ExitDuration, AccelerateKeySpline)
                }
            };
            Storyboard.SetTargetProperty(opacityAnim, OpacityPath);
            storyboard.Children.Add(opacityAnim);
        }

        return new(element, storyboard);
    }
}