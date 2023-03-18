using System.Runtime.CompilerServices;

namespace DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

public static class CharExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToLowerFastRusEng(this char c)
    {
        return c switch
        {
            >= 'A' and <= 'Z' => (char) (c - 65 + 97),
            >= 'А' and <= 'Я' => (char) (c - 1040 + 1072),
            _ => c == 'Ё' ? 'ё' : c,
        };
    }
}