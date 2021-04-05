using System.Collections.Generic;
using GenshinLyreMidiPlayer.ModernWPF.Animation.Transitions;

namespace GenshinLyreMidiPlayer.ModernWPF.Animation
{
    public class TransitionCollection : List<CaptionedObject<Transition>>
    {
        public TransitionCollection()
        {
            Add(new CaptionedObject<Transition>(new EntranceTransition(), "Entrance"));
            Add(new CaptionedObject<Transition>(new DrillInTransition(), "Drill in"));
            Add(new CaptionedObject<Transition>(new SlideTransition(Direction.FromLeft), "Slide from Left"));
            Add(new CaptionedObject<Transition>(new SlideTransition(Direction.FromRight), "Slide from Right"));
            Add(new CaptionedObject<Transition>(new SlideTransition(Direction.FromBottom), "Slide from Bottom"));
            Add(new CaptionedObject<Transition>(new SuppressTransition(), "Suppress"));
        }
    }
}