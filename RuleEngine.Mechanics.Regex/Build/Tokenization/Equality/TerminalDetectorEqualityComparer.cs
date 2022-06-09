using System;
using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Lib.Common.Helpers;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

namespace RuleEngine.Mechanics.Regex.Build.Tokenization.Equality;

internal sealed class TerminalDetectorEqualityComparer : IEqualityComparer<ITerminalDetector>
{
    public static readonly TerminalDetectorEqualityComparer Instance = new();

    private static readonly DelegateEqualityComparer<(LiteralSetDetector.MemberType Type, string Value)> MemberEqualityComparer = new(
        (x, y) => x.Type == y.Type && x.Value == y.Value,
        obj =>
        {
            var hashCode = obj.Type.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.Value.GetHashCode();
            return hashCode;
        }
    );

    private TerminalDetectorEqualityComparer()
    {
    }

    public bool Equals(ITerminalDetector? x, ITerminalDetector? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x switch
        {
            AnyLiteralDetector => true,
            LiteralDetector detector => detector.Literal == (y as LiteralDetector)!.Literal,
            InfixDetector detector => detector.Infix == (y as InfixDetector)!.Infix,
            PrefixDetector detector => detector.Prefix == (y as PrefixDetector)!.Prefix,
            SuffixDetector detector => detector.Suffix == (y as SuffixDetector)!.Suffix,
            // todo [realtime performance] this can be improved by introducing order-ignoring comparison, as [a b] is actually equal to [b a]
            LiteralSetDetector detector => detector.IsNegative == (y as LiteralSetDetector)!.IsNegative && detector.Members.SequenceEqual((y as LiteralSetDetector)!.Members, MemberEqualityComparer),
            _ => throw new ArgumentOutOfRangeException(nameof(x)),
        };
    }

    public int GetHashCode(ITerminalDetector obj)
    {
        return obj switch
        {
            AnyLiteralDetector => typeof(AnyLiteralDetector).GetHashCode(),
            LiteralDetector detector => detector.Literal.GetHashCode(),
            InfixDetector detector => detector.Infix.GetHashCode(),
            PrefixDetector detector => detector.Prefix.GetHashCode(),
            SuffixDetector detector => detector.Suffix.GetHashCode(),
            LiteralSetDetector detector => detector.IsNegative.GetHashCode() * 17 + detector.Members.GetSequenceHashCode(MemberEqualityComparer),
            _ => throw new ArgumentOutOfRangeException(nameof(obj)),
        };
    }
}