namespace DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

public static class ArrayExtension
{
    public static TValue[]? NullIfEmpty<TValue>(this TValue[] array)
    {
        return array.Length == 0 ? null : array;
    }
}