using System.Collections.Generic;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Animation.Transitions;

public static class TransitionCollection
{
    public static List<CaptionedObject<Transition>> Transitions = new()
    {
        new(new EntranceTransition(), "输入"),
        new(new DrillInTransition(), "键入"),
        new(new SlideTransition(Direction.FromLeft), "从左侧滑动"),
        new(new SlideTransition(Direction.FromRight), "从右侧滑动"),
        new(new SlideTransition(Direction.FromBottom), "从底部滑动"),
        new(new SuppressTransition(), "平")
    };
}