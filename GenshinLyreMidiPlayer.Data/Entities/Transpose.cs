using System.ComponentModel;

namespace GenshinLyreMidiPlayer.Data.Entities;

public enum Transpose
{
    [Description("忽略遗漏的记录")] Ignore,
    [Description("向上移调")] Up,
    [Description("向下移调")] Down
}