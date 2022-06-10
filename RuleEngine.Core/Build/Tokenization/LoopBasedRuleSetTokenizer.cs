using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Exceptions;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization;

namespace RuleEngine.Core.Build.Tokenization;

// todo [code quality] refactor this method to use RuleSetTokenizationException (instead of base one)
internal sealed class LoopBasedRuleSetTokenizer : IRuleSetTokenizer
{
    private readonly IReadOnlyDictionary<string, IPatternTokenizer> _patternTokenizers;
    private readonly CSharpCodeTokenizer _cSharpCodeTokenizer;

    public LoopBasedRuleSetTokenizer(IReadOnlyDictionary<string, IPatternTokenizer> patternTokenizers)
    {
        _patternTokenizers = patternTokenizers;
        _cSharpCodeTokenizer = new CSharpCodeTokenizer();
    }

    public RuleSetToken Tokenize(string ruleSet, string? @namespace, bool caseSensitive)
    {
        ruleSet = ruleSet.ReplaceLineEndings();

        var i = 0;
        // todo [non-realtime performance] get rid of this (now it's needed to speed-up IronMeta)
        var chars = ruleSet.ToList();
        var usings = ParseUsings(ruleSet, chars, ref i);
        var rules = ParseRules(ruleSet, chars, @namespace, caseSensitive, ref i);

        if (i != ruleSet.Length)
        {
            throw new RuleEngineTokenizationException("Unable to parse rule set.", ruleSet);
        }

        return new RuleSetToken(usings, rules);
    }

    private RuleToken[] ParseRules(
        string ruleSet,
        List<char> chars,
        string? @namespace,
        bool caseSensitive,
        ref int i
    )
    {
        var usedRuleNames = new HashSet<string>();
        var rules = new List<RuleToken>();

        _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref i, "in very beginning");

        while (i < ruleSet.Length)
        {
            var type = _cSharpCodeTokenizer.TokenizeCSharpType(ruleSet, chars, ref i);

            _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref i, "after rule result type", true);

            var ruleName = CSharpCodeTokenizer.ParseIdentifier(ruleSet, ref i, "rule name");

            if (!usedRuleNames.Add(ruleName))
            {
                throw new RuleEngineTokenizationException($"Duplicate rule '{ruleName}'.", ruleSet);
            }

            _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref i, "after rule name");

            var ruleParameters = _cSharpCodeTokenizer.ParseParameters(ruleSet, chars, ref i, "rule parameters");

            _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref i, "after rule parameters");

            if (i >= ruleSet.Length || ruleSet[i] != '=')
            {
                throw new RuleEngineTokenizationException($"Unmatched character '='.{CSharpCodeTokenizer.GetDetails(ruleSet, i)}", ruleSet);
            }

            i++;

            _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref i, "after '=' sign");

            var patternKey = CSharpCodeTokenizer.ParseIdentifier(ruleSet, ref i, "pattern key");

            _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref i, "after pattern key");

            var pattern = ReadPattern(patternKey, ref i);

            _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref i, "after pattern");

            var projection = ReadProjection(ref i);

            _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref i, "after projection");

            rules.Add(new RuleToken(@namespace, type, ruleName, ruleParameters, patternKey, pattern, projection));
        }

        return rules.ToArray();

        IPatternToken ReadPattern(
            string patternKey,
            ref int index
        )
        {
            if (index >= ruleSet.Length || ruleSet[index] != '#')
            {
                throw new RuleEngineTokenizationException($"Unmatched pattern start.{CSharpCodeTokenizer.GetDetails(ruleSet, index)}", ruleSet);
            }

            index++;

            var startIndex = index;
            while (ruleSet[index] != '#')
            {
                if (index < ruleSet.Length - 1)
                {
                    index++;
                }
                else
                {
                    throw new RuleEngineTokenizationException($"Unmatched pattern end.{CSharpCodeTokenizer.GetDetails(ruleSet, index + 1)}", ruleSet);
                }
            }
            var length = index - startIndex;

            // todo [non-realtime performance] this can be improved by making patter tokenizer accept start index and return end one
            var pattern = _patternTokenizers[patternKey].Tokenize(
                ruleSet.Substring(startIndex, length),
                @namespace,
                caseSensitive
            );

            index++;

            return pattern;
        }

        IProjectionToken ReadProjection(ref int index)
        {
            if (index + 1 < ruleSet.Length && ruleSet[index] == '=' && ruleSet[index + 1] == '>')
            {
                index += 2;
                _cSharpCodeTokenizer.ConsumeWhitespaces(ruleSet, chars, ref index, "in non-c# projection");

                return _cSharpCodeTokenizer.ParseConstantProjection(ruleSet, ref index);
            }

            var body = _cSharpCodeTokenizer.ParseMethodBody(ruleSet, chars, ref index);

            if (body == "{}")
            {
                return VoidProjectionToken.Instance;
            }

            return new BodyBasedProjectionToken(body);
        }
    }

    private UsingToken[] ParseUsings(string ruleSet, List<char> chars, ref int i)
    {
        var usings = new List<UsingToken>();

        while (i < ruleSet.Length)
        {
            var c = ruleSet[i++];

            switch (c)
            {
                case ' ':
                case '\r':
                case '\n':
                case '\t':
                {
                    continue;
                }
                case 'u':
                {
                    i--;
                    usings.Add(_cSharpCodeTokenizer.ParseUsing(ruleSet, chars, ref i));
                    break;
                }
                default:
                {
                    i--;

                    return usings.ToArray();
                }
            }
        }

        throw new RuleEngineTokenizationException("Unable to read usings.", ruleSet);
    }
}