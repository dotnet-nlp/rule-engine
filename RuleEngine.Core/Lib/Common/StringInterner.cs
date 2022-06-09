using System.Collections.Concurrent;

namespace RuleEngine.Core.Lib.Common;

public sealed class StringInterner
{
    private readonly ConcurrentDictionary<string, string> _knownStrings;

    public StringInterner()
    {
        _knownStrings = new ConcurrentDictionary<string, string>();
    }

    public string InternString(string @string)
    {
        return _knownStrings.GetOrAdd(@string, @string);
    }

    public void Clear()
    {
        _knownStrings.Clear();
    }
}