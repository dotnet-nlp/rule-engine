using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.Transitions;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Label;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Visualization;

internal sealed class RegexAutomatonLabelProvider : ILabelProvider<RegexAutomatonState, RegexAutomatonTransition>
{
    public static readonly ILabelProvider<RegexAutomatonState, RegexAutomatonTransition> Instance = new RegexAutomatonLabelProvider();

    public string GetLabel(RegexAutomatonState vertex)
    {
        return $"[{vertex.Id}]";
    }

    public string GetLabel(RegexAutomatonTransition edge)
    {
        var label = edge.Payload switch
        {
            EpsilonPayload => "ε",
            MarkerPayload payload => $"「{payload.Marker}」",
            NerPayload payload => FormatNerPayload(payload),
            RuleReferencePayload payload => FormatReferencePayload(payload),
            TerminalPayload payload => FormatTerminalPayload(payload),
            VariableCapturePayload payload => $":{payload.VariableName}",
            _ => throw new ArgumentOutOfRangeException(nameof(edge)),
        };

        return $"{label} ({edge.Id})";
    }

    private static string FormatNerPayload(NerPayload payload)
    {
        return $"{FormatRule(payload.RuleSpaceKey, payload.RuleArguments)} (extract)";
    }

    private static string FormatReferencePayload(RuleReferencePayload payload)
    {
        return $"{FormatRule(payload.RuleSpaceKey, payload.RuleArguments)} (forward)";
    }

    private static string FormatRule(string ruleKey, IReadOnlyCollection<IRuleArgumentToken> ruleArguments)
    {
        var formattedPayload = ruleKey;

        if (ruleArguments.Count > 0)
        {
            formattedPayload = $"{formattedPayload}({ruleArguments.Select(FormatRuleArgument).JoinToString(", ")})";
        }

        return formattedPayload;

        string FormatRuleArgument(IRuleArgumentToken ruleArgument)
        {
            return ruleArgument switch
            {
                RuleChainedMemberAccessArgumentToken token => token.ToString(),
                RuleDefaultArgumentToken => "default",
                _ => throw new ArgumentOutOfRangeException(nameof(ruleArgument)),
            };
        }
    }

    private static string FormatTerminalPayload(TerminalPayload payload)
    {
        return payload.TerminalDetector switch
        {
            AnyLiteralDetector => ".",
            LiteralDetector detector => FormatLiteral(detector.Literal),
            PrefixDetector detector => FormatPrefix(detector.Prefix),
            InfixDetector detector => FormatInfix(detector.Infix),
            SuffixDetector detector => FormatSuffix(detector.Suffix),
            LiteralSetDetector detector => $"[{(detector.IsNegative ? "^" : "")}{detector.Members.Select(FormatLiteralSetMember).JoinToString(" ")}]",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private static string FormatLiteralSetMember((LiteralSetDetector.MemberType Type, string Value) member)
    {
        Func<string, string> formatter = member.Type switch
        {
            LiteralSetDetector.MemberType.Literal => FormatLiteral,
            LiteralSetDetector.MemberType.Prefix => FormatPrefix,
            LiteralSetDetector.MemberType.Infix => FormatInfix,
            LiteralSetDetector.MemberType.Suffix => FormatSuffix,
            _ => throw new ArgumentOutOfRangeException(),
        };

        return formatter(member.Value);
    }

    private static string FormatLiteral(string literal)
    {
        return literal;
    }

    private static string FormatPrefix(string prefix)
    {
        return $"{prefix}~";
    }

    private static string FormatInfix(string infix)
    {
        return $"~{infix}~";
    }

    private static string FormatSuffix(string suffix)
    {
        return $"~{suffix}";
    }
}