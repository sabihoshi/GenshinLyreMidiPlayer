using System.Collections.Generic;
using GenshinLyreMidiPlayer.WPF.ModernWPF.Animation.Transitions;

namespace GenshinLyreMidiPlayer.WPF.ModernWPF.Animation
{
    public class TransitionCollection : List<CaptionedObject<Transition>>
    {
        public TransitionCollection()
        {
            With(new EntranceTransition(), "Entrance");
            With(new DrillInTransition(), "Drill in");
            With(new SlideTransition(Direction.FromLeft), "Slide from Left");
            With(new SlideTransition(Direction.FromRight), "Slide from Right");
            With(new SlideTransition(Direction.FromBottom), "Slide from Bottom");
            With(new SuppressTransition(), "Suppress");

            void With(Transition transition, string caption) =>
                Add(new CaptionedObject<Transition>(transition, caption));
        }
    }
}