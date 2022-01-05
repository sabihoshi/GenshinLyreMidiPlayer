using System.Collections.Generic;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Animation.Transitions;

public static class TransitionCollection
{
    public static List<CaptionedObject<Transition>> Transitions = new()
    {
        new(new EntranceTransition(), "Entrance"),
        new(new DrillInTransition(), "Drill in"),
        new(new SlideTransition(Direction.FromLeft), "Slide from Left"),
        new(new SlideTransition(Direction.FromRight), "Slide from Right"),
        new(new SlideTransition(Direction.FromBottom), "Slide from Bottom"),
        new(new SuppressTransition(), "Suppress")
    };
}