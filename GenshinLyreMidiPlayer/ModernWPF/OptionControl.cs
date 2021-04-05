using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace GenshinLyreMidiPlayer.ModernWPF
{
    [ContentProperty(nameof(Content))]
    public class OptionControl : Control
    {
        static OptionControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OptionControl),
                new FrameworkPropertyMetadata(typeof(OptionControl)));
        }

        #region HeaderText

        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(
                nameof(HeaderText),
                typeof(string),
                typeof(OptionControl),
                new PropertyMetadata(string.Empty));

        public string HeaderText
        {
            get => (string) GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        #endregion

        #region Content

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(object),
                typeof(OptionControl),
                null);

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        #endregion

        #region Options

        public static readonly DependencyProperty OptionsProperty =
            DependencyProperty.Register(
                nameof(Options),
                typeof(object),
                typeof(OptionControl),
                null);

        public object Options
        {
            get => GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        #endregion

        #region MaxContentWidth

        public static readonly DependencyProperty MaxContentWidthProperty =
            DependencyProperty.Register(
                nameof(MaxContentWidth),
                typeof(double),
                typeof(OptionControl),
                new PropertyMetadata(1028d));

        public double MaxContentWidth
        {
            get => (double) GetValue(MaxContentWidthProperty);
            set => SetValue(MaxContentWidthProperty, value);
        }

        #endregion
    }
}