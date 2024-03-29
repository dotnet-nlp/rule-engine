﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens.Internal;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using IronMeta.Matcher;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization;

// todo [non-realtime performance] get rid of chars (now it's needed to speed-up IronMeta)
public sealed class CSharpCodeTokenizer
{
    private const string IdentifierPattern = "[a-zA-Z_][a-zA-Z0-9_]*";
    private static readonly Regex IdentifierRegex = new($"{IdentifierPattern}", RegexOptions.Compiled);
    private static readonly Regex NamespaceRegex = new($"{IdentifierPattern}(?:\\.{IdentifierPattern})*", RegexOptions.Compiled);
    private static readonly Regex IntRegex = new("\\d+", RegexOptions.Compiled);
    private static readonly Regex StringRegex = new("\"([^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

    private readonly ErrorIndexHelper _errorIndexHelper;
    private readonly CSharpSyntaxMatcher _cSharpSyntaxMatcher;

    public CSharpCodeTokenizer(ErrorIndexHelper errorIndexHelper)
    {
        _errorIndexHelper = errorIndexHelper;
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
                $"Failed to parse c# type.{_errorIndexHelper.GetDetails(source, result.ErrorIndex)}",
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
    public string ParseNamespace(string source, ref int index)
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

    public string ParseIdentifier(string source, ref int index, string reference)
    {
        var match = IdentifierRegex.Match(source, index);

        if (!match.Success)
        {
            throw new RuleEngineTokenizationException(
                $"Failed to parse c# identifier in {reference} at all.{_errorIndexHelper.GetDetails(source, index)}",
                source
            );
        }

        if (match.Index != index)
        {
            throw new RuleEngineTokenizationException(
                $"Failed to parse c# identifier in {reference}.{_errorIndexHelper.GetDetails(source, index)}",
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
                        $"Failed to parse whitespace {reference}.{_errorIndexHelper.GetDetails(source, index)}",
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
                $"Failed to parse whitespace after {reference}.{_errorIndexHelper.GetDetails(source, index)}",
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
                $"Failed to parse c# method body.{_errorIndexHelper.GetDetails(source, bodyMatchResult.ErrorIndex)}",
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
            $"{_errorIndexHelper.GetDetails(source, index)}",
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
                $"Invalid using declaration.{_errorIndexHelper.GetDetails(source, index)}",
                source
            );
        }

        ConsumeWhitespaces(source, chars, ref index, "after using keyword");

        var @namespace = ParseNamespace(source, ref index);

        if (index >= source.Length || source[index] != ';')
        {
            throw new RuleEngineTokenizationException(
                $"Using declaration has missing ';'.{_errorIndexHelper.GetDetails(source, index)}",
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