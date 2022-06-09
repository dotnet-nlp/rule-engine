using System.Collections.Generic;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;
using RuleEngine.Mechanics.Regex.Exceptions;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class LiteralSetDetector : ITerminalDetector
{
    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly bool IsNegative;

    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly (MemberType Type, string Value)[] Members;

    public LiteralSetDetector(LiteralSetToken literalSet)
    {
        IsNegative = literalSet.IsNegative;
        Members = CreateMembers();

        (MemberType Type, string Value)[] CreateMembers()
        {
            var members = new (MemberType Type, string Value)[literalSet.Members.Length];

            for (var index = 0; index < literalSet.Members.Length; index++)
            {
                var member = literalSet.Members[index];

                members[index] = member switch
                {
                    LiteralToken literalToken => (MemberType.Literal, literalToken.Literal),
                    PrefixToken prefixToken => (MemberType.Prefix, prefixToken.Prefix),
                    InfixToken infixToken => (MemberType.Infix, infixToken.Infix),
                    SuffixToken suffixToken => (MemberType.Suffix, suffixToken.Suffix),
                    _ => throw new RegexProcessorMatchException($"Unknown literal set member type {member.GetType().FullName}."),
                };
            }

            return members;
        }
    }

    public bool WordMatches(string word, out int explicitlyMatchedSymbolsCount)
    {
        var hasPositiveMatch = HasPositiveMatch(word);

        var isMatched = IsNegative != hasPositiveMatch;

        explicitlyMatchedSymbolsCount = isMatched && !IsNegative ? 1 : 0;

        return isMatched;
    }

    private bool HasPositiveMatch(string word)
    {
        foreach (var member in Members)
        {
            var memberMatched = member.Type switch
            {
                MemberType.Literal => LiteralDetector.WordMatches(member.Value, word),
                MemberType.Prefix => PrefixDetector.WordMatches(member.Value, word),
                MemberType.Infix => InfixDetector.WordMatches(member.Value, word),
                MemberType.Suffix => SuffixDetector.WordMatches(member.Value, word),
                _ => throw new RegexProcessorMatchException($"Unknown literal set member type {member.Type.ToString()}."),
            };

            if (memberMatched)
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerable<string> GetUsedWords()
    {
        foreach (var member in Members)
        {
            if (member.Type == MemberType.Literal)
            {
                yield return member.Value;
            }
        }
    }

    public enum MemberType : byte
    {
        Literal,
        Prefix,
        Infix,
        Suffix,
    }
}