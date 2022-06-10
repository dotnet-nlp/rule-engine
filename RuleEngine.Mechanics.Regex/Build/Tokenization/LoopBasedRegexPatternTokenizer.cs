using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RuleEngine.Core.Build.Tokenization;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization;
using RuleEngine.Core.Lib.Common;
using RuleEngine.Core.Lib.Common.Helpers;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;
using RuleEngine.Mechanics.Regex.Exceptions;

namespace RuleEngine.Mechanics.Regex.Build.Tokenization;

public sealed class LoopBasedRegexPatternTokenizer : IPatternTokenizer
{
    private readonly StringInterner _stringInterner;
    private readonly ErrorIndexHelper _errorIndexHelper;
    private readonly QuantifierReader _quantifierReader;
    private readonly NerReader _nerReader;

    public LoopBasedRegexPatternTokenizer(StringInterner stringInterner)
    {
        _stringInterner = stringInterner;
        _errorIndexHelper = new ErrorIndexHelper(Environment.NewLine);
        _quantifierReader = new QuantifierReader(_errorIndexHelper);
        _nerReader = new NerReader(_errorIndexHelper);
    }

    public IPatternToken Tokenize(string pattern, string? @namespace, bool caseSensitive)
    {
        pattern = pattern.ReplaceLineEndings();

        var usedMarkers = new HashSet<string>();

        var groups = new Stack<List<List<IBranchItemToken>>>();

        List<List<IBranchItemToken>> currentBranchItems;

        CreateNewCurrentGroup();

        var lastReadTokenIsQuantifier = false;
        var i = 0;
        while (i < pattern.Length)
        {
            var c = caseSensitive ? pattern[i++] : pattern[i++].ToLowerFastRusEng();

            switch (c)
            {
                case '(':
                {
                    groups.Push(currentBranchItems);

                    CreateNewCurrentGroup();

                    lastReadTokenIsQuantifier = false;
                    break;
                }
                case '|':
                {
                    ThrowIfBranchIsEmpty(LastBranchOfCurrentGroup(), pattern, i - 1);

                    AddNewBranchToCurrentGroup();

                    lastReadTokenIsQuantifier = false;

                    break;
                }
                case ')':
                {
                    ThrowIfBranchIsEmpty(LastBranchOfCurrentGroup(), pattern, i - 1);

                    var closedGroup = TryGatherLexemesFromBranchesToGroup(currentBranchItems);

                    if (groups.Count < 1)
                    {
                        throw new RegexPatternTokenizationException(
                            $"Too many closing brackets.{_errorIndexHelper.GetDetails(pattern, i - 1)}",
                            pattern
                        );
                    }

                    currentBranchItems = groups.Pop();

                    AddItemToCurrentBranch(closedGroup);

                    lastReadTokenIsQuantifier = false;

                    break;
                }
                case '*':
                {
                    QuantifyLastBranchItem(new QuantifierToken(0, null));

                    lastReadTokenIsQuantifier = true;

                    break;
                }
                case '+':
                {
                    QuantifyLastBranchItem(new QuantifierToken(1, null));

                    lastReadTokenIsQuantifier = true;

                    break;
                }
                case '?':
                {
                    QuantifyLastBranchItem(new QuantifierToken(0, 1));

                    lastReadTokenIsQuantifier = true;

                    break;
                }
                case '{':
                {
                    i--;
                    var firstQuantifierCharIndex = i;
                    QuantifyLastBranchItem(_quantifierReader.ReadQuantifier(pattern, ref i), firstQuantifierCharIndex);

                    lastReadTokenIsQuantifier = true;

                    break;
                }
                case MarkerToken.MarkerStart:
                {
                    i--;
                    LastBranchOfCurrentGroup().Add(ParseMarker(pattern, ref i, usedMarkers));

                    lastReadTokenIsQuantifier = false;

                    break;
                }
                case '.':
                {
                    AddItemToCurrentBranch(AnyLiteralToken.Instance);

                    ThrowIfCharIsLetter(pattern, i);

                    lastReadTokenIsQuantifier = false;

                    break;
                }
                case '[':
                {
                    IQuantifiableToken literalSet = ReadLiteralsSet(pattern, ref i, caseSensitive);
                    AddItemToCurrentBranch(literalSet);

                    lastReadTokenIsQuantifier = false;

                    break;
                }
                case '<':
                {
                    i--;
                    var ner = _nerReader.ReadNerDeclaration(pattern, ref i, @namespace);

                    AddItemToCurrentBranch(
                        ner.Reference,
                        ner.VariableName
                    );

                    lastReadTokenIsQuantifier = false;

                    break;
                }
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                {
                    break;
                }
                case '~':
                {
                    var word = ParseWordLiteral(pattern, ref i, caseSensitive);
                    if (i < pattern.Length && pattern[i] == '~')
                    {
                        ++i;
                        AddItemToCurrentBranch(new InfixToken(word));
                        ThrowIfCharIsLetter(pattern, i);
                    }
                    else
                    {
                        AddItemToCurrentBranch(new SuffixToken(word));
                    }

                    lastReadTokenIsQuantifier = false;

                    break;
                }
                default:
                {
                    if (IsCharRussian(c) || IsCharEngOrDigit(c))
                    {
                        --i;
                        var word = ParseWordLiteral(pattern, ref i, caseSensitive);
                        if (i < pattern.Length && pattern[i] == '~')
                        {
                            ++i;
                            AddItemToCurrentBranch(new PrefixToken(word));
                            ThrowIfCharIsLetter(pattern, i);
                        }
                        else
                        {
                            AddItemToCurrentBranch(new LiteralToken(word));
                        }
                    }
                    else
                    {
                        throw new RegexPatternTokenizationException(
                            $"Unexpected char.{_errorIndexHelper.GetDetails(pattern, i - 1)}",
                            pattern
                        );
                    }

                    lastReadTokenIsQuantifier = false;

                    break;
                }
            }
        }

        var currentGroup = TryGatherLexemesFromBranchesToGroup(currentBranchItems, true);

        if (groups.Count > 0)
        {
            throw new RegexPatternTokenizationException(
                $"Not closed open parentheses.{_errorIndexHelper.GetDetails(pattern, i - 1)}",
                pattern
            );
        }

        return currentGroup;

        void CreateNewCurrentGroup()
        {
            currentBranchItems = new List<List<IBranchItemToken>>();
            AddNewBranchToCurrentGroup();
        }

        void AddNewBranchToCurrentGroup()
        {
            currentBranchItems.Add(new List<IBranchItemToken>());
        }

        void AddItemToCurrentBranch(IQuantifiableToken quantifiable, string? variableName = null)
        {
            LastBranchOfCurrentGroup().Add(new QuantifiableBranchItemToken(quantifiable, new QuantifierToken(1, 1), variableName));
        }

        void QuantifyLastBranchItem(QuantifierToken quantifier, int? firstQuantifierChar = null)
        {
            firstQuantifierChar ??= i - 1;

            ThrowIfNotInQuantifierPosition(LastBranchOfCurrentGroup(), pattern, firstQuantifierChar.Value, lastReadTokenIsQuantifier);

            var lastBranchOfCurrentGroup = LastBranchOfCurrentGroup();
            var lastBranchItemIndex = lastBranchOfCurrentGroup.Count - 1;
            var lastBranchItem = lastBranchOfCurrentGroup[lastBranchItemIndex];

            if (lastBranchItem is QuantifiableBranchItemToken quantifiableBranchItem)
            {
                lastBranchOfCurrentGroup[lastBranchItemIndex] = new QuantifiableBranchItemToken(
                    quantifiableBranchItem.Quantifiable,
                    quantifier,
                    null
                );
            }
            else
            {
                throw new RegexPatternTokenizationException(
                    $"Cannot apply quantifier '{quantifier}': " +
                    $"last branch item '{lastBranchItem.ToString()}' is not quantifiable." +
                    $"{_errorIndexHelper.GetDetails(pattern, firstQuantifierChar.Value)}",
                    pattern
                );
            }

            ThrowIfCharIsLetter(pattern, i);
        }

        List<IBranchItemToken> LastBranchOfCurrentGroup()
        {
            return currentBranchItems[^1];
        }
    }

    private void ThrowIfBranchIsEmpty(ICollection currentBranchItems, string pattern, int i)
    {
        if (currentBranchItems.Count == 0)
        {
            throw new RegexPatternTokenizationException(
                $"Empty branches are not allowed.{_errorIndexHelper.GetDetails(pattern, i)}",
                pattern
            );
        }
    }

    private static RegexGroupToken TryGatherLexemesFromBranchesToGroup(IReadOnlyList<List<IBranchItemToken>> branches, bool simplify = false)
    {
        if (simplify && branches.Count == 1 && branches[0].Count == 1)
        {
            var branchItem = branches[0][0];

            if (branchItem is QuantifiableBranchItemToken { Quantifiable: RegexGroupToken group } quantifiableBranchItem &&
                quantifiableBranchItem.Quantifier.Min == 1 && quantifiableBranchItem.Quantifier.Max == 1
               )
            {
                return group;
            }
        }

        return new RegexGroupToken(branches.Select(branch => new BranchToken(branch.ToArray())).ToArray());
    }

    private void ThrowIfNotInQuantifierPosition(
        IEnumerable<IBranchItemToken> currentBranchItems,
        string pattern,
        int quantifierFirstCharIndex,
        bool lastReadTokenIsQuantifier
    )
    {
        var branchItem = currentBranchItems.LastOrDefault();

        if (branchItem is not QuantifiableBranchItemToken || lastReadTokenIsQuantifier)
        {
            throw new RegexPatternTokenizationException(
                $"Quantifier is only allowed after quantifiable lexeme." +
                $"{_errorIndexHelper.GetDetails(pattern, quantifierFirstCharIndex)}",
                pattern
            );
        }

        var previousChar = pattern[quantifierFirstCharIndex - 1];

        if (previousChar is ' ' or '\t' or '\n' or '\r')
        {
            throw new RegexPatternTokenizationException(
                $"Quantifier must stand right after the lexeme it's quantifying." +
                $"{_errorIndexHelper.GetDetails(pattern, quantifierFirstCharIndex)}",
                pattern
            );
        }
    }

    private void ThrowIfCharIsLetter(string pattern, int i)
    {
        if (i >= pattern.Length)
        {
            return;
        }

        var c = pattern[i];

        if (IsCharRussian(c) || IsCharEngOrDigit(c))
        {
            throw new RegexPatternTokenizationException(
                $"Found word start in invalid position.{_errorIndexHelper.GetDetails(pattern, i)}",
                pattern
            );
        }
    }

    private static bool IsCharRussian(char c)
    {
        return c
            is >= 'а' and <= 'я'
            or 'ё'
            or '-'
            or >= 'А' and <= 'Я'
            or 'Ё';
    }

    private static bool IsCharEngOrDigit(char c)
    {
        return c
            is >= 'a' and <= 'z'
            or '-'
            or '\''
            or >= '0' and <= '9'
            or >= 'A' and <= 'Z';
    }

    private MarkerToken ParseMarker(string pattern, ref int i, ISet<string> usedMarkers)
    {
        var initialPosition = i;

        ++i;
        while (i < pattern.Length && pattern[i] != MarkerToken.MarkerEnd)
        {
            ++i;
        }

        if (i == pattern.Length - 1 && pattern[i] != MarkerToken.MarkerEnd)
        {
            throw new RegexPatternTokenizationException(
                $"Not found end of marker.{_errorIndexHelper.GetDetails(pattern, initialPosition)}",
                pattern
            );
        }

        var markerLength = i - initialPosition - 1;
        if (markerLength == 0)
        {
            throw new RegexPatternTokenizationException(
                $"Empty marker.{_errorIndexHelper.GetDetails(pattern, initialPosition)}",
                pattern
            );
        }

        ++i;

        var marker = pattern.Substring(initialPosition + 1, markerLength);
        if (!usedMarkers.Add(marker))
        {
            throw new RegexPatternTokenizationException(
                $"Duplicated marker '{marker}'.{_errorIndexHelper.GetDetails(pattern, initialPosition)}",
                pattern
            );
        }

        return new MarkerToken(marker);
    }

    private LiteralSetToken ReadLiteralsSet(string pattern, ref int i, bool caseSensitive)
    {
        var startIndex = i - 1;
        bool negate;
        var members = new List<ILiteralSetMemberToken>();
        if (i < pattern.Length && pattern[i] == '^')
        {
            negate = true;
            ++i;
        }
        else
        {
            negate = false;
        }

        while (i < pattern.Length)
        {
            var c = caseSensitive ? pattern[i++] : pattern[i++].ToLowerFastRusEng();

            switch (c)
            {
                case ']':
                {
                    return new LiteralSetToken(negate, members.ToArray());
                }
                case '[':
                case '^':
                case '\\':
                {
                    throw new RegexPatternTokenizationException(
                        $"Unhandled char in literal set.{_errorIndexHelper.GetDetails(pattern, i - 1)}",
                        pattern
                    );
                }
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                {
                    break;
                }
                case '~':
                {
                    var word = ParseWordLiteral(pattern, ref i, caseSensitive);
                    if (i < pattern.Length && pattern[i] == '~')
                    {
                        ++i;
                        members.Add(new InfixToken(word));

                        ThrowIfCharIsLetter(pattern, i);
                    }
                    else
                    {
                        members.Add(new SuffixToken(word));
                    }
                    break;
                }
                default:
                {
                    if (IsCharRussian(c) || IsCharEngOrDigit(c))
                    {
                        --i;
                        var word = ParseWordLiteral(pattern, ref i, caseSensitive);
                        if (i < pattern.Length && pattern[i] == '~')
                        {
                            ++i;
                            members.Add(new PrefixToken(word));
                            ThrowIfCharIsLetter(pattern, i);
                        }
                        else
                        {
                            members.Add(new LiteralToken(word));
                        }
                    }
                    else
                    {
                        throw new RegexPatternTokenizationException(
                            $"Unhandled char in literal set.{_errorIndexHelper.GetDetails(pattern, i - 1)}",
                            pattern
                        );
                    }

                    break;
                }
            }
        }

        throw new RegexPatternTokenizationException(
            $"Unbound character set.{_errorIndexHelper.GetDetails(pattern, startIndex)}",
            pattern
        );
    }

    private string ParseWordLiteral(string pattern, ref int i, bool caseSensitive)
    {
        if (i >= pattern.Length)
        {
            throw new RegexPatternTokenizationException(
                $"Empty word.{_errorIndexHelper.GetDetails(pattern, i)}",
                pattern
            );
        }

        var startIndex = i;
        //Check for first symbol to declare language
        var isRussianLanguageWordDeclare = IsCharRussian(pattern[i]);

        if (isRussianLanguageWordDeclare)
        {
            //Check word till symbol not in Russian
            while (i < pattern.Length && IsCharRussian(pattern[i]))
            {
                ++i;
            }

            //Check next symbol to opposite language (it would English or Digit symbol)
            if (i < pattern.Length && IsCharEngOrDigit(pattern[i]))
            {
                throw new RegexPatternTokenizationException(
                    $"Unhandled char. Check word for different language usage." +
                    $"{_errorIndexHelper.GetDetails(pattern, i)}",
                    pattern
                );
            }
        }
        else
        {
            //Check word till symbol not in English or Digit
            while (i < pattern.Length && IsCharEngOrDigit(pattern[i]))
            {
                ++i;
            }

            //Check next symbol to opposite language (it would Russian)
            if (i < pattern.Length && IsCharRussian(pattern[i]))
            {
                throw new RegexPatternTokenizationException(
                    $"Unhandled char. Check word for different language usage." +
                    $"{_errorIndexHelper.GetDetails(pattern, i)}",
                    pattern
                );
            }
        }

        var word = pattern.Substring(startIndex, i - startIndex);

        if (!caseSensitive)
        {
            word = word.ToLowerFastRusEng();
        }

        if (word.Length == 0)
        {
            throw new RegexPatternTokenizationException(
                $"Empty word.{_errorIndexHelper.GetDetails(pattern, startIndex)}",
                pattern
            );
        }

        word = _stringInterner.InternString(word);

        return word;
    }

    private sealed class QuantifierReader
    {
        private readonly ErrorIndexHelper _errorIndexHelper;

        public QuantifierReader(ErrorIndexHelper errorIndexHelper)
        {
            _errorIndexHelper = errorIndexHelper;
        }

        /// <summary>
        /// This method reads quantifiers {n,m}, {n}, {n,}
        /// </summary>
        /// <param name="pattern">String with regular expression to process</param>
        /// <param name="i">Index of the quantifier opening character "{"</param>
        /// <returns>Quantifier lexeme</returns>
        public QuantifierToken ReadQuantifier(string pattern, ref int i)
        {
            var initialPosition = i;
            var nStrBuilder = new StringBuilder();
            var mStrBuilder = new StringBuilder();
            var reachedComma = false;
            var reachedClosing = false;
            var noCharactersAfterComma = false;

            // We are not interested in '{' opening character
            i++;

            while (i < pattern.Length && !reachedClosing)
            {
                var c = pattern[i++];
                switch (c)
                {
                    case ',':
                        reachedComma = true;
                        noCharactersAfterComma = true;
                        break;
                    case '}':
                        reachedClosing = true;
                        break;
                    default:
                        if (reachedComma)
                        {
                            noCharactersAfterComma = false;
                            mStrBuilder.Append(c);
                        }
                        else
                        {
                            nStrBuilder.Append(c);
                        }
                        break;
                }
            }

            if (!reachedClosing)
            {
                throw new RegexPatternTokenizationException(
                    $"Unmatched opening bracket '{{'.{_errorIndexHelper.GetDetails(pattern, initialPosition)}",
                    pattern
                );
            }

            var nStr = nStrBuilder.ToString();
            if (!int.TryParse(nStr, out var n))
            {
                throw new RegexPatternTokenizationException(
                    $"Expected an integer parameter.{_errorIndexHelper.GetDetails(pattern, initialPosition + 1)}",
                    pattern
                );
            }

#pragma warning disable S2583
            if (noCharactersAfterComma)
#pragma warning restore S2583
            {
                return ConstructNonLessThanQuantifier(n, pattern, initialPosition);
            }

#pragma warning disable S2583
            if (reachedComma)
#pragma warning restore S2583
            {
                var mStr = mStrBuilder.ToString();
                if (!int.TryParse(mStr, out var m))
                {
                    throw new RegexPatternTokenizationException(
                        $"Expected an integer parameter." +
                        $"{_errorIndexHelper.GetDetails(pattern, initialPosition + 1)}",
                        pattern
                    );
                }

                return ConstructRangeQuantifier(n, m, pattern, initialPosition);
            }

            return ConstructExactQuantifier(n, pattern, initialPosition);
        }

        private QuantifierToken ConstructRangeQuantifier(int n, int m, string pattern, int position)
        {
            if (n < 0)
            {
                throw new RegexPatternTokenizationException(
                    $"Cannot create a quantifier {{{n},{m}}} with n < 0." +
                    $"{_errorIndexHelper.GetDetails(pattern, position)}",
                    pattern
                );
            }

            if (m < n)
            {
                throw new RegexPatternTokenizationException(
                    $"Cannot create a quantifier {{{n},{m}}} with n > m." +
                    $"{_errorIndexHelper.GetDetails(pattern, position)}",
                    pattern
                );
            }

            return new QuantifierToken(n, m);
        }

        private QuantifierToken ConstructExactQuantifier(int n, string pattern, int position)
        {
            if (n < 1)
            {
                throw new RegexPatternTokenizationException(
                    $"Cannot create a quantifier {{{n}}} with n < 1." +
                    $"{_errorIndexHelper.GetDetails(pattern, position)}",
                    pattern
                );
            }

            return new QuantifierToken(n, n);
        }

        private QuantifierToken ConstructNonLessThanQuantifier(int n, string pattern, int position)
        {
            if (n < 0)
            {
                throw new RegexPatternTokenizationException(
                    $"Cannot create a quantifier {{{n},}} with n < 0." +
                    $"{_errorIndexHelper.GetDetails(pattern, position)}",
                    pattern
                );
            }

            return new QuantifierToken(n, null);
        }
    }

    private sealed class NerReader
    {
        private static readonly System.Text.RegularExpressions.Regex GeneralNerRegex = new(@"<\s*(?:(?<var>[a-zA-Z_][a-zA-Z_0-9]*)\s*=\s*)?(?<call>[a-zA-Z_][a-zA-Z_0-9]*(?:\.[a-zA-Z_][a-zA-Z_0-9]*)*)(?:\s*\(\s*(?:(?<arg>(?:default|[a-zA-Z_][a-zA-Z_0-9]*(?:\.[a-zA-Z_][a-zA-Z_0-9]*)*))(?:\s*,\s*(?<arg>(?:default|[a-zA-Z_][a-zA-Z_0-9]*(?:\.[a-zA-Z_][a-zA-Z_0-9]*)*)))*)?\s*\))?\s*>", RegexOptions.Compiled);

        private readonly ErrorIndexHelper _errorIndexHelper;

        public NerReader(ErrorIndexHelper errorIndexHelper)
        {
            _errorIndexHelper = errorIndexHelper;
        }

        public (RuleReferenceToken Reference, string? VariableName) ReadNerDeclaration(string pattern, ref int position, string? @namespace)
        {
            var match = GeneralNerRegex.Match(pattern, position);
            if (!match.Success)
            {
                throw new RegexPatternTokenizationException(
                    $"Failed to parse NER inclusion at all.{_errorIndexHelper.GetDetails(pattern, position)}",
                    pattern
                );
            }
            if (match.Index != position)
            {
                throw new RegexPatternTokenizationException(
                    $"Failed to parse NER inclusion.{_errorIndexHelper.GetDetails(pattern, position)}",
                    pattern
                );
            }

            position = match.Index + match.Length;

            var variableName = match.Groups["var"].Success ? match.Groups["var"].Value : null;
            var callChain = match.Groups["call"].Value;
            var arguments = match
                .Groups["arg"]
                .Captures
#if NETFRAMEWORK
                    .Cast<Capture>()
#endif
                .Select<Capture, IRuleArgumentToken>(
                    capture =>
                    {
                        if (capture.Value == "default")
                        {
                            return RuleDefaultArgumentToken.Instance;
                        }

                        return new RuleChainedMemberAccessArgumentToken(
                            capture.Value.Split('.', StringSplitOptions.RemoveEmptyEntries)
                        );
                    }
                )
                .ToArray();

            return (new RuleReferenceToken(@namespace, callChain, arguments), variableName);
        }
    }
}