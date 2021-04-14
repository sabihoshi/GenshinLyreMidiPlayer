using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WindowsInput.Native;

namespace GenshinLyreMidiPlayer.Core
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static class Keyboard
    {
        public enum Layout
        {
            QWERTY,
            QWERTZ,
            AZERTY,
            DVORAK,
            DVORAKLeft,
            DVORAKRight,
            Colemak
        }

        public static readonly Dictionary<Layout, string> LayoutNames = new()
        {
            [Layout.QWERTY]      = "QWERTY",
            [Layout.QWERTZ]      = "QWERTZ",
            [Layout.AZERTY]      = "AZERTY",
            [Layout.DVORAK]      = "DVORAK",
            [Layout.DVORAKLeft]  = "DVORAK Left Handed",
            [Layout.DVORAKRight] = "DVORAK Right Handed",
            [Layout.Colemak]     = "Colemak"
        };

        private static readonly IReadOnlyList<VirtualKeyCode> QWERTY = new List<VirtualKeyCode>
        {
            VirtualKeyCode.VK_Z,
            VirtualKeyCode.VK_X,
            VirtualKeyCode.VK_C,
            VirtualKeyCode.VK_V,
            VirtualKeyCode.VK_B,
            VirtualKeyCode.VK_N,
            VirtualKeyCode.VK_M,

            VirtualKeyCode.VK_A,
            VirtualKeyCode.VK_S,
            VirtualKeyCode.VK_D,
            VirtualKeyCode.VK_F,
            VirtualKeyCode.VK_G,
            VirtualKeyCode.VK_H,
            VirtualKeyCode.VK_J,

            VirtualKeyCode.VK_Q,
            VirtualKeyCode.VK_W,
            VirtualKeyCode.VK_E,
            VirtualKeyCode.VK_R,
            VirtualKeyCode.VK_T,
            VirtualKeyCode.VK_Y,
            VirtualKeyCode.VK_U
        };

        private static readonly IReadOnlyList<VirtualKeyCode> QWERTZ = new List<VirtualKeyCode>
        {
            VirtualKeyCode.VK_Y,
            VirtualKeyCode.VK_X,
            VirtualKeyCode.VK_C,
            VirtualKeyCode.VK_V,
            VirtualKeyCode.VK_B,
            VirtualKeyCode.VK_N,
            VirtualKeyCode.VK_M,

            VirtualKeyCode.VK_A,
            VirtualKeyCode.VK_S,
            VirtualKeyCode.VK_D,
            VirtualKeyCode.VK_F,
            VirtualKeyCode.VK_G,
            VirtualKeyCode.VK_H,
            VirtualKeyCode.VK_J,

            VirtualKeyCode.VK_Q,
            VirtualKeyCode.VK_W,
            VirtualKeyCode.VK_E,
            VirtualKeyCode.VK_R,
            VirtualKeyCode.VK_T,
            VirtualKeyCode.VK_Z,
            VirtualKeyCode.VK_U
        };

        private static readonly IReadOnlyList<VirtualKeyCode> AZERTY = new List<VirtualKeyCode>
        {
            VirtualKeyCode.VK_Z,
            VirtualKeyCode.VK_X,
            VirtualKeyCode.VK_C,
            VirtualKeyCode.VK_V,
            VirtualKeyCode.VK_B,
            VirtualKeyCode.VK_N,
            VirtualKeyCode.OEM_COMMA,

            VirtualKeyCode.VK_Q,
            VirtualKeyCode.VK_S,
            VirtualKeyCode.VK_D,
            VirtualKeyCode.VK_F,
            VirtualKeyCode.VK_G,
            VirtualKeyCode.VK_H,
            VirtualKeyCode.VK_J,

            VirtualKeyCode.VK_A,
            VirtualKeyCode.VK_Z,
            VirtualKeyCode.VK_E,
            VirtualKeyCode.VK_R,
            VirtualKeyCode.VK_T,
            VirtualKeyCode.VK_Y,
            VirtualKeyCode.VK_U
        };

        private static readonly IReadOnlyList<VirtualKeyCode> DVORAK = new List<VirtualKeyCode>
        {
            VirtualKeyCode.OEM_2,
            VirtualKeyCode.VK_B,
            VirtualKeyCode.VK_I,
            VirtualKeyCode.OEM_PERIOD,
            VirtualKeyCode.VK_N,
            VirtualKeyCode.VK_L,
            VirtualKeyCode.VK_M,

            VirtualKeyCode.VK_A,
            VirtualKeyCode.OEM_1,
            VirtualKeyCode.VK_H,
            VirtualKeyCode.VK_Y,
            VirtualKeyCode.VK_U,
            VirtualKeyCode.VK_J,
            VirtualKeyCode.VK_C,

            VirtualKeyCode.VK_X,
            VirtualKeyCode.OEM_COMMA,
            VirtualKeyCode.VK_D,
            VirtualKeyCode.VK_O,
            VirtualKeyCode.VK_K,
            VirtualKeyCode.VK_T,
            VirtualKeyCode.VK_F
        };

        private static readonly IReadOnlyList<VirtualKeyCode> DVORAKLeft = new List<VirtualKeyCode>
        {
            VirtualKeyCode.VK_L,
            VirtualKeyCode.VK_X,
            VirtualKeyCode.VK_D,
            VirtualKeyCode.VK_V,
            VirtualKeyCode.VK_E,
            VirtualKeyCode.VK_N,
            VirtualKeyCode.VK_6,

            VirtualKeyCode.VK_K,
            VirtualKeyCode.VK_U,
            VirtualKeyCode.VK_F,
            VirtualKeyCode.VK_5,
            VirtualKeyCode.VK_C,
            VirtualKeyCode.VK_H,
            VirtualKeyCode.VK_8,

            VirtualKeyCode.VK_W,
            VirtualKeyCode.VK_B,
            VirtualKeyCode.VK_J,
            VirtualKeyCode.VK_Y,
            VirtualKeyCode.VK_G,
            VirtualKeyCode.VK_R,
            VirtualKeyCode.VK_T
        };

        private static readonly IReadOnlyList<VirtualKeyCode> DVORAKRight = new List<VirtualKeyCode>
        {
            VirtualKeyCode.VK_D,
            VirtualKeyCode.VK_C,
            VirtualKeyCode.VK_L,
            VirtualKeyCode.OEM_COMMA,
            VirtualKeyCode.VK_P,
            VirtualKeyCode.VK_N,
            VirtualKeyCode.VK_7,

            VirtualKeyCode.VK_F,
            VirtualKeyCode.VK_U,
            VirtualKeyCode.VK_K,
            VirtualKeyCode.VK_8,
            VirtualKeyCode.OEM_PERIOD,
            VirtualKeyCode.VK_H,
            VirtualKeyCode.VK_5,

            VirtualKeyCode.VK_E,
            VirtualKeyCode.VK_M,
            VirtualKeyCode.VK_G,
            VirtualKeyCode.VK_Y,
            VirtualKeyCode.VK_J,
            VirtualKeyCode.VK_O,
            VirtualKeyCode.VK_I
        };

        private static readonly IReadOnlyList<VirtualKeyCode> Colemak = new List<VirtualKeyCode>
        {
            VirtualKeyCode.VK_Z,
            VirtualKeyCode.VK_X,
            VirtualKeyCode.VK_C,
            VirtualKeyCode.VK_V,
            VirtualKeyCode.VK_B,
            VirtualKeyCode.VK_J,
            VirtualKeyCode.VK_M,

            VirtualKeyCode.VK_A,
            VirtualKeyCode.VK_D,
            VirtualKeyCode.VK_G,
            VirtualKeyCode.VK_E,
            VirtualKeyCode.VK_T,
            VirtualKeyCode.VK_H,
            VirtualKeyCode.VK_Y,

            VirtualKeyCode.VK_Q,
            VirtualKeyCode.VK_W,
            VirtualKeyCode.VK_K,
            VirtualKeyCode.VK_S,
            VirtualKeyCode.VK_F,
            VirtualKeyCode.VK_O,
            VirtualKeyCode.VK_I
        };

        public static IReadOnlyList<VirtualKeyCode> GetLayout(Layout layout)
        {
            return layout switch
            {
                Layout.QWERTY      => QWERTY,
                Layout.QWERTZ      => QWERTZ,
                Layout.AZERTY      => AZERTY,
                Layout.DVORAK      => DVORAK,
                Layout.DVORAKLeft  => DVORAKLeft,
                Layout.DVORAKRight => DVORAKRight,
                Layout.Colemak     => Colemak,
                _                  => QWERTY
            };
        }
    }
}