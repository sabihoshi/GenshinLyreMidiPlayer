#nullable enable
using System;

namespace GenshinLyreMidiPlayer.ModernWPF
{
    public class CaptionedObject<T>
    {
        public CaptionedObject(T o, string? caption = null)
        {
            Object  = o;
            Caption = caption;
        }

        public string? Caption { get; }

        public T Object { get; }

        public override string ToString()
        {
            return Caption ?? base.ToString() ?? string.Empty;
        }
    }

    public class CaptionedObject<T, TEnum> : CaptionedObject<T> where T : Enum
    {
        private readonly CaptionedObject<T> _caption;

        public CaptionedObject(T o, TEnum type, string? caption = null) : base(o, caption)
        {
            _caption = new CaptionedObject<T>(o, caption);
            Type     = type;
        }

        public TEnum Type { get; }

        public override string ToString()
        {
            return Caption ?? Type?.ToString() ?? base.ToString();
        }
    }
}