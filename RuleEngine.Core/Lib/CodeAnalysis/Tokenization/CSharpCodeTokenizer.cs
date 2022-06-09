using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronMeta.Matcher;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Exceptions;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens.Internal;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Lib.CodeAnalysis.Tokenization;

// todo [non-realtime performance] get rid of chars (now it's needed to speed-up IronMeta)
public sealed class CSharpCodeTokenizer
{
    private const string IdentifierPattern = "[a-zA-Z_][a-zA-Z0-9_]*";
    private static readonly Regex IdentifierRegex = new($"{IdentifierPattern}", RegexOptions.Compiled);
    private static readonly Regex NamespaceRegex = new($"{IdentifierPattern}(?:\\.{IdentifierPattern})*", RegexOptions.Compiled);
    private static readonly Regex IntRegex = new("\\d+", RegexOptions.Compiled);
    private static readonly Regex StringRegex = new("\"([^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

    private readonly CSharpSyntaxMatcher _cSharpSyntaxMatcher;

    public CSharpCodeTokenizer()
    {
        _cSharpSyntaxMatcher = new CSharpSyntaxMatcher();
    }

    public ICSharpTypeToken TokenizeCSharpType(string source, List<char> chars, ref int index)
    {
        MatchResult<char, IToken> result = _cSharpSyntaxMatcher.GetMatch(
            chars,
            _cSharpSyntaxMatcher.CSharpType,
            index
        );

        if (!result.Success)
        {
            throw new RuleEngineTokenizationException(
                $"Failed to parse c# type.{GetDetails(source, result.ErrorIndex)}",
                source
            );
        }

        if (result.Result is null)
        {
            throw new RuleEngineTokenizationException("C# type token is null.", source);
        }

        if (result.Result is not ICSharpTypeToken cSharpTypeToken)
        {
            throw new RuleEngineTokenizationException($"C# type token is of unexpected type '{result.Result.GetType().Name}'.", source);
        }

        index = result.NextIndex;

        return cSharpTypeToken;
    }

    // todo whitespace tolerance could be added here
    public static string ParseNamespace(string source, ref int index)
    {
        var match = NamespaceRegex.Match(source, index);

        if (!match.Success)
        {
            throw new RuleEngineTokenizationException(
                "Failed to parse c# namespace at all.",
                source
            );
        }

        if (match.Index != index)
        {
            throw new RuleEngineTokenizationException(
                "Failed to parse c# namespace.",
                source
            );
        }

        index = match.Index + match.Length;

        return match.Value;
    }

    public static string ParseIdentifier(string source, ref int index, string reference)
    {
        var match = IdentifierRegex.Match(source, index);

        if (!match.Success)
        {
            throw new RuleEngineTokenizationException(
                $"Failed to parse c# identifier in {reference} at all.{GetDetails(source, index)}",
                source
            );
        }

        if (match.Index != index)
        {
            throw new RuleEngineTokenizationException(
                $"Failed to parse c# identifier in {reference}.{GetDetails(source, index)}",
                source
            );
        }

        index = match.Index + match.Length;

        return match.Value;
    }

    public void ConsumeWhitespaces(string source, List<char> chars, ref int index, string reference, bool required = false)
    {
        var startIndex = index;

        while (index < source.Length)
        {
            var c = source[index];

            if (c is ' ' or '\t' or '\r' or '\n')
            {
                index++;
            }
            else if (c is '/')
            {
                var commentStartIndex = index;

                ReadComment(source, chars, ref index);

                if (index == commentStartIndex)
                {
                    throw new RuleEngineTokenizationException(
                        $"Failed to parse whitespace {reference}.{GetDetails(source, index)}",
                        source
                    );
                }
            }
            else
            {
                break;
            }
        }

        if (required && startIndex == index)
        {
            throw new RuleEngineTokenizationException(
                $"Failed to parse whitespace after {reference}.{GetDetails(source, index)}",
                source
            );
        }
    }

    public void ReadComment(string source, List<char> chars, ref int index)
    {
        MatchResult<char, IToken> result = _cSharpSyntaxMatcher.GetMatch(
            chars,
            _cSharpSyntaxMatcher.CSharpComment,
            index
        );

        if (result.Success)
        {
            index = result.NextIndex;
        }
    }

    public CSharpParameterToken[] ParseParameters(string source, List<char> chars, ref int index, string reference)
    {
        if (index >= source.Length || source[index] != '(')
        {
            return Array.Empty<CSharpParameterToken>();
        }

        index++;

        ConsumeWhitespaces(source, chars, ref index, $"in {reference}");

        var parameters = new List<CSharpParameterToken>();

        while (index < source.Length)
        {
            parameters.Add(ParseParameter(source, chars, ref index, reference));

            ConsumeWhitespaces(source, chars, ref index, $"in {reference}");

            if (source[index] == ')')
            {
                index++;
                return parameters.ToArray();
            }

            if (source[index] == ',')
            {
                index++;
                ConsumeWhitespaces(source, chars, ref index, $"in {reference}");
            }
        }

        throw new RuleEngineTokenizationException("Cannot parse parameters.", source);
    }

    private CSharpParameterToken ParseParameter(string source, List<char> chars, ref int index, string reference)
    {
        var typeToken = TokenizeCSharpType(source, chars, ref index);

        ConsumeWhitespaces(source, chars, ref index, reference, true);

        var name = ParseIdentifier(source, ref index, reference);

        return new CSharpParameterToken(typeToken, name);
    }

    public string ParseMethodBody(string source, List<char> chars, ref int index)
    {
        MatchResult<char, IToken> emptyBodyMatchResult = _cSharpSyntaxMatcher.GetMatch(
            chars,
            _cSharpSyntaxMatcher.CSharpEmptyMethodBody,
            index
        );

        if (emptyBodyMatchResult.Success)
        {
            index = emptyBodyMatchResult.NextIndex;

            return "{}";
        }

        MatchResult<char, IToken> bodyMatchResult = _cSharpSyntaxMatcher.GetMatch(
            chars,
            _cSharpSyntaxMatcher.CSharpMethodBody,
            index
        );

        if (!bodyMatchResult.Success)
        {
            throw new RuleEngineTokenizationException(
                $"Failed to parse c# method body.{GetDetails(source, bodyMatchResult.ErrorIndex)}",
                source
            );
        }

        if (bodyMatchResult.Result is null)
        {
            throw new RuleEngineTokenizationException("C# method body is null.", source);
        }

        if (bodyMatchResult.Result is not ContainerToken<string> bodyContainer)
        {
            throw new RuleEngineTokenizationException($"C# method body token is of unexpected type '{bodyMatchResult.Result.GetType().Name}'.", source);
        }

        index = bodyMatchResult.NextIndex;

        return bodyContainer.Value;
    }

    public static string GetDetails(string pattern, int position)
    {
        var errorContext = ErrorIndexHelper.FindContext(pattern, position, 10);

        var details = string.Empty;

        if (errorContext.Highlighter != null)
        {
            return $" " +
                   $"Absolute position: {position}. " +
                   $"Line: {errorContext.LineIndex}; position in line: {errorContext.PositionInLine}. " +
                   $"Near character: '{errorContext.Highlighter.ErrorCharacter}'. " +
                   $"Context: '{errorContext.Highlighter.StringAroundError}'.";
        }

        return details;
    }

    public IProjectionToken ParseConstantProjection(string source, ref int index)
    {
        if (TryParseWord("void", source, ref index))
        {
            return VoidProjectionToken.Instance;
        }

        if (TryParseInt(source, ref index, out var @int))
        {
            return new ConstantProjectionToken(@int);
        }

        if (TryParseString(source, ref index, out var @string))
        {
            return new ConstantProjectionToken(@string);
        }

        throw new RuleEngineTokenizationException(
            $"Cannot parse non-c# projection {index}." +
            $"{GetDetails(source, index)}",
            source
        );
    }

    private static bool TryParseInt(string source, ref int index, out int result)
    {
        var match = IntRegex.Match(source, index);

        if (!match.Success)
        {
            result = default;
            return false;
        }

        if (match.Index != index)
        {
            result = default;
            return false;
        }

        index = match.Index + match.Length;

        result = int.Parse(match.Value);
        return true;
    }

    private static bool TryParseString(
        string source,
        ref int index,
        out string? result
    )
    {
        var match = StringRegex.Match(source, index);

        if (!match.Success)
        {
            result = default;
            return false;
        }

        if (match.Index != index)
        {
            result = default;
            return false;
        }

        index = match.Index + match.Length;

        result = match.Value.Substring(1, match.Value.Length - 2);
        return true;
    }

    public UsingToken ParseUsing(string source, List<char> chars, ref int index)
    {
        if (!TryParseWord("using", source, ref index))
        {
            throw new RuleEngineTokenizationException(
                $"Invalid using declaration.{GetDetails(source, index)}",
                source
            );
        }

        ConsumeWhitespaces(source, chars, ref index, "after using keyword");

        var @namespace = ParseNamespace(source, ref index);

        if (index >= source.Length || source[index] != ';')
        {
            throw new RuleEngineTokenizationException(
                $"Using declaration has missing ';'.{GetDetails(source, index)}",
                source
            );
        }

        index++;

        return new UsingToken(@namespace);
    }

    private static bool TryParseWord(string word, string source, ref int index)
    {
        var localIndex = index;
        for (var wordIndex = 0; wordIndex < word.Length; wordIndex++)
        {
            if (localIndex >= source.Length || source[localIndex] != word[wordIndex])
            {
                return false;
            }

            localIndex++;
        }

        index = localIndex;

        return true;
    }
}