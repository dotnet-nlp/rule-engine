using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Rule.Static.Attributes;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Tests.Fixtures;

[StaticRuleContainer("ner")]
public static class DummyStaticNerContainer
{
    [StaticRule("digit", nameof(GetUsedWords))]
    public static (bool success, int result, int lastUserSymbolIndex) ParseDigit(string[] sequence, int startIndex)
    {
        var digit = TryParseDigit(sequence[startIndex]);

        return digit is not null ? (true, digit.Value, startIndex) : (false, default, default);
    }

    [StaticRule("digit_with_salt", nameof(GetUsedWords))]
    public static (bool success, string? result, int lastUsedSymbolIndex) ParseDigitWithSalt(
        string[] sequence,
        int startIndex,
        string? salt = null,
        int? saltRepeatTimes = null
    )
    {
        var digit = TryParseDigit(sequence[startIndex]);

        return digit is not null
            ? (true, $"digit_{digit.Value}_salt_{Enumerable.Repeat(salt ?? "no salt", saltRepeatTimes ?? 1).JoinToString(string.Empty)}", startIndex)
            : (false, default, default);
    }

    [StaticRule("any_arguments", nameof(GetUsedWords))]
    public static (bool success, string? result, int lastUsedSymbolIndex) ParseAnyArguments(
        string[] sequence,
        int startIndex,
        string? stringArgument = null,
        int? intArgument = null
    )
    {
        var extractedStringArgument = sequence[startIndex++];

        if (extractedStringArgument != stringArgument)
        {
            return (false, default, default);
        }

        var extractedIntArgument = TryParseDigit(sequence[startIndex]);

        if (extractedIntArgument != intArgument)
        {
            return (false, default, default);
        }

        return (true, $"{intArgument} {stringArgument}", startIndex);
    }

    [StaticRule("number_line", nameof(GetUsedWords))]
    public static IEnumerable<(int result, int lastUserSymbolIndex)> ParseNumberLine(string[] sequence, int startIndex)
    {
        for (var index = startIndex; index < sequence.Length; index++)
        {
            var digit = TryParseDigit(sequence[index]);

            if (digit is null)
            {
                yield break;
            }

            yield return (digit.Value, index);
        }
    }

    private static int? TryParseDigit(string symbol)
    {
        return symbol switch
        {
            "ноль" => 0,
            "один" => 1,
            "два" => 2,
            "три" => 3,
            "четыре" => 4,
            "пять" => 5,
            "шесть" => 6,
            "семь" => 7,
            "восемь" => 8,
            "девять" => 9,
            _ => null,
        };
    }

    private static IEnumerable<string> GetUsedWords()
    {
        return Enumerable.Empty<string>();
    }
}