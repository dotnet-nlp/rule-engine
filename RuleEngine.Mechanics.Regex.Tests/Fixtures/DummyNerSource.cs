using System;
using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Build.Rule.Static.Attributes;

namespace RuleEngine.Mechanics.Regex.Tests.Fixtures;

[StaticRuleContainer("dummy_ner")]
public static class DummyNerSource
{
    [StaticRule("hi", nameof(GetUsedWords))]
    public static (bool success, string? result, int lastUserSymbolIndex) ParseHi(string[] sequence, int startIndex)
    {
        var hi = new[] { "привет", "здравствуй" };

        return hi.Contains(sequence[startIndex]) ? (true, "приветствие", startIndex) : (false, null, 0);
    }

    [StaticRule("bye", nameof(GetUsedWords))]
    public static (bool success, string? result, int lastUserSymbolIndex) ParseBye(string[] sequence, int startIndex)
    {
        var bye = new[] { "пока", "прощай" };

        return bye.Contains(sequence[startIndex]) ? (true, "прощание", startIndex) : (false, null, 0);
    }

    [StaticRule("two", nameof(GetUsedWords))]
    public static (bool success, int result, int lastUserSymbolIndex) ParseTwo(string[] sequence, int startIndex)
    {
        return sequence[startIndex] == "два" ? (true, 2, startIndex) : (false, 0, 0);
    }

    [StaticRule("three_with_optional_four", nameof(GetUsedWords))]
    public static (bool success, int[] result, int lastUserSymbolIndex) ParseThreeWithOptionalFour(string[] sequence, int startIndex)
    {
        var expectedSequence = new[] { "три", "четыре" };
        var expectedSequenceNer = new[] { 3, 4 };

        var success = false;
        var result = new int[2];
        var lastUserSymbolIndex = 0;

        var expectedSequenceIndex = 0;
        for (var sequenceIndex = startIndex; sequenceIndex < sequence.Length; sequenceIndex++, expectedSequenceIndex++)
        {
            if (expectedSequenceIndex >= expectedSequence.Length)
            {
                break;
            }

            if (expectedSequence[expectedSequenceIndex] != sequence[sequenceIndex])
            {
                break;
            }

            success = true;
            result[expectedSequenceIndex] = expectedSequenceNer[expectedSequenceIndex];
            lastUserSymbolIndex = sequenceIndex;
        }

        return (success, result, lastUserSymbolIndex);
    }

    [StaticRule("any_next_two_words", nameof(GetUsedWords))]
    public static (bool success, string? result, int lastUserSymbolIndex) ParseAnyTwoWords(string[] sequence, int startIndex)
    {
        return (true, "два слова", Math.Min(startIndex + 1, sequence.Length - 1));
    }

    [StaticRule("any_next_three_words", nameof(GetUsedWords))]
    public static (bool success, string? result, int lastUserSymbolIndex) ParseAnyThreeWords(string[] sequence, int startIndex)
    {
        return (true, "три слова", Math.Min(startIndex + 2, sequence.Length - 1));
    }

    [StaticRule("hi_star", nameof(GetUsedWords))]
    public static (bool success, string? result, int lastUserSymbolIndex) ParseHiStar(string[] sequence, int startIndex)
    {
        var isMatched = false;
        var lastUserSymbolIndex = 0;
        for (var sequenceIndex = startIndex; sequenceIndex < sequence.Length; sequenceIndex++)
        {
            if ("привет" == sequence[sequenceIndex])
            {
                isMatched = true;
                lastUserSymbolIndex = sequenceIndex;
            }
            else
            {
                break;
            }
        }

        return (isMatched, isMatched ? "приветствие" : null, lastUserSymbolIndex);
    }

    private static IEnumerable<string> GetUsedWords()
    {
        return Enumerable.Empty<string>();
    }
}