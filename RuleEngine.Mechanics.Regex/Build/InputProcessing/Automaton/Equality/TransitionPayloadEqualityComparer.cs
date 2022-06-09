using System;
using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Build.Tokenization.Equality;
using RuleEngine.Core.Lib.Common.Helpers;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Equality;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

namespace RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Equality;

internal sealed class TransitionPayloadEqualityComparer : IEqualityComparer<ITransitionPayload>
{
    public static readonly TransitionPayloadEqualityComparer Instance = new();

    public bool Equals(ITransitionPayload? x, ITransitionPayload? y)
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
            EpsilonPayload => true,
            MarkerPayload payload => MarkerPayloadEqualityComparer.Instance.Equals(payload, y as MarkerPayload),
            NerPayload payload => NerPayloadEqualityComparer.Instance.Equals(payload, y as NerPayload),
            RuleReferencePayload payload => RuleReferencePayloadEqualityComparer.Instance.Equals(payload, y as RuleReferencePayload),
            TerminalPayload payload => TerminalPayloadEqualityComparer.Instance.Equals(payload, y as TerminalPayload),
            VariableCapturePayload payload => VariableCapturePayloadEqualityComparer.Instance.Equals(payload, y as VariableCapturePayload),
            _ => throw new ArgumentOutOfRangeException(nameof(x)),
        };
    }

    public int GetHashCode(ITransitionPayload obj)
    {
        return obj switch
        {
            EpsilonPayload => typeof(EpsilonPayload).GetHashCode(),
            MarkerPayload payload => MarkerPayloadEqualityComparer.Instance.GetHashCode(payload),
            NerPayload payload => NerPayloadEqualityComparer.Instance.GetHashCode(payload),
            RuleReferencePayload payload => RuleReferencePayloadEqualityComparer.Instance.GetHashCode(payload),
            TerminalPayload payload => TerminalPayloadEqualityComparer.Instance.GetHashCode(payload),
            VariableCapturePayload payload => VariableCapturePayloadEqualityComparer.Instance.GetHashCode(payload),
            _ => throw new ArgumentOutOfRangeException(nameof(obj)),
        };
    }

    private class MarkerPayloadEqualityComparer : IEqualityComparer<MarkerPayload>
    {
        public static readonly MarkerPayloadEqualityComparer Instance = new();

        private MarkerPayloadEqualityComparer()
        {
        }

        public bool Equals(MarkerPayload? x, MarkerPayload? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Marker == y.Marker;
        }

        public int GetHashCode(MarkerPayload obj)
        {
            return obj.Marker.GetHashCode();
        }
    }

    private class NerPayloadEqualityComparer : IEqualityComparer<NerPayload>
    {
        public static readonly NerPayloadEqualityComparer Instance = new();

        private NerPayloadEqualityComparer()
        {
        }

        public bool Equals(NerPayload? x, NerPayload? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.RuleSpaceKey == y.RuleSpaceKey &&
                   x.RuleArguments.SequenceEqual(y.RuleArguments, RuleArgumentTokenEqualityComparer.Instance);
        }

        public int GetHashCode(NerPayload obj)
        {
            var hashCode = obj.RuleSpaceKey.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.RuleArguments.GetSequenceHashCode(RuleArgumentTokenEqualityComparer.Instance);
            return hashCode;
        }
    }

    private class RuleReferencePayloadEqualityComparer : IEqualityComparer<RuleReferencePayload>
    {
        public static readonly RuleReferencePayloadEqualityComparer Instance = new();

        private RuleReferencePayloadEqualityComparer()
        {
        }

        public bool Equals(RuleReferencePayload? x, RuleReferencePayload? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.RuleSpaceKey == y.RuleSpaceKey &&
                   x.RuleArguments.SequenceEqual(y.RuleArguments, RuleArgumentTokenEqualityComparer.Instance);
        }

        public int GetHashCode(RuleReferencePayload obj)
        {
            var hashCode = obj.RuleSpaceKey.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.RuleArguments.GetSequenceHashCode(RuleArgumentTokenEqualityComparer.Instance);
            return hashCode;
        }
    }

    private class TerminalPayloadEqualityComparer : IEqualityComparer<TerminalPayload>
    {
        public static readonly TerminalPayloadEqualityComparer Instance = new();

        private TerminalPayloadEqualityComparer()
        {
        }

        public bool Equals(TerminalPayload? x, TerminalPayload? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return TerminalDetectorEqualityComparer.Instance.Equals(x.TerminalDetector, y.TerminalDetector);
        }

        public int GetHashCode(TerminalPayload obj)
        {
            return TerminalDetectorEqualityComparer.Instance.GetHashCode(obj.TerminalDetector);
        }
    }

    private class VariableCapturePayloadEqualityComparer : IEqualityComparer<VariableCapturePayload>
    {
        public static readonly VariableCapturePayloadEqualityComparer Instance = new();

        private VariableCapturePayloadEqualityComparer()
        {
        }

        public bool Equals(VariableCapturePayload? x, VariableCapturePayload? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.VariableName == y.VariableName;
        }

        public int GetHashCode(VariableCapturePayload obj)
        {
            return obj.VariableName.GetHashCode();
        }
    }
}