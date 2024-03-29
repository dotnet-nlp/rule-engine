﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Equality;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Equality;

public sealed class RuleSetTokenEqualityComparer : IEqualityComparer<RuleSetToken>
{
    private readonly RuleTokenEqualityComparer _ruleTokenEqualityComparer;

    public RuleSetTokenEqualityComparer(IEqualityComparer<IPatternToken> patternTokenEqualityComparer)
    {
        _ruleTokenEqualityComparer = new RuleTokenEqualityComparer(patternTokenEqualityComparer);
    }

    public bool Equals(RuleSetToken? x, RuleSetToken? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
        {
            return false;
        }

        return x.Usings.SequenceEqual(y.Usings, UsingTokenEqualityComparer.Instance) &&
               x.Rules.SequenceEqual(y.Rules, _ruleTokenEqualityComparer);
    }

    public int GetHashCode(RuleSetToken obj)
    {
        return (obj.Usings.GetSequenceHashCode(UsingTokenEqualityComparer.Instance) * 397) ^ obj.Rules.GetSequenceHashCode(_ruleTokenEqualityComparer);
    }

    private sealed class UsingTokenEqualityComparer : IEqualityComparer<UsingToken>, IEqualityComparer
    {
        public static readonly UsingTokenEqualityComparer Instance = new();

        private UsingTokenEqualityComparer()
        {
        }

        bool IEqualityComparer.Equals(object? x, object? y)
        {
            return Equals(x as UsingToken, y as UsingToken);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj is UsingToken token ? GetHashCode(token) : 0;
        }

        public bool Equals(UsingToken? x, UsingToken? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Namespace == y.Namespace;
        }

        public int GetHashCode(UsingToken obj)
        {
            return obj.Namespace.GetHashCode();
        }
    }

    // todo feature generalize for IRuleToken
    private sealed class RuleTokenEqualityComparer : IEqualityComparer<RuleToken>, IEqualityComparer
    {
        private readonly IEqualityComparer<IPatternToken> _patternTokenEqualityComparer;

        public RuleTokenEqualityComparer(IEqualityComparer<IPatternToken> patternTokenEqualityComparer)
        {
            _patternTokenEqualityComparer = patternTokenEqualityComparer;
        }

        bool IEqualityComparer.Equals(object? x, object? y)
        {
            return Equals(x as UsingToken, y as UsingToken);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj is RuleToken token ? GetHashCode(token) : 0;
        }

        public bool Equals(RuleToken? x, RuleToken? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Namespace == y.Namespace &&
                   x.Name == y.Name &&
                   x.PatternKey == y.PatternKey &&
                   CSharpTypeTokenEqualityComparer.Instance.Equals(x.ReturnType, y.ReturnType) &&
                   _patternTokenEqualityComparer.Equals(x.Pattern, y.Pattern) &&
                   ProjectionTokenEqualityComparer.Instance.Equals(x.Projection, y.Projection) &&
                   x.RuleParameters.SequenceEqual(y.RuleParameters, RuleParameterTokenEqualityComparer.Instance);
        }

        public int GetHashCode(RuleToken obj)
        {
            var hashCode = obj.Namespace?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ obj.Name.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.PatternKey.GetHashCode();
            hashCode = (hashCode * 397) ^ CSharpTypeTokenEqualityComparer.Instance.GetHashCode(obj.ReturnType);
            hashCode = (hashCode * 397) ^ _patternTokenEqualityComparer.GetHashCode(obj.Pattern);
            hashCode = (hashCode * 397) ^ ProjectionTokenEqualityComparer.Instance.GetHashCode(obj.Projection);
            hashCode = (hashCode * 397) ^ obj.RuleParameters.GetSequenceHashCode(RuleParameterTokenEqualityComparer.Instance);
            return hashCode;
        }
    }

    private sealed class RuleParameterTokenEqualityComparer : EqualityComparer<CSharpParameterToken>
    {
        public static readonly RuleParameterTokenEqualityComparer Instance = new();

        public override bool Equals(CSharpParameterToken? x, CSharpParameterToken? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Name == y.Name &&
                   CSharpTypeTokenEqualityComparer.Instance.Equals(x.Type, y.Type);
        }

        public override int GetHashCode(CSharpParameterToken obj)
        {
            var hashCode = obj.Name.GetHashCode();
            hashCode = (hashCode * 397) ^ CSharpTypeTokenEqualityComparer.Instance.GetHashCode(obj.Type);
            return hashCode;
        }
    }
    private sealed class ProjectionTokenEqualityComparer : IEqualityComparer<IProjectionToken>
    {
        public static readonly ProjectionTokenEqualityComparer Instance = new();

        private ProjectionTokenEqualityComparer()
        {
        }

        public bool Equals(IProjectionToken? x, IProjectionToken? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x switch
            {
                BodyBasedProjectionToken token => BodyBasedProjectionTokenEqualityComparer.Instance.Equals(token, y as BodyBasedProjectionToken),
                MatchedInputBasedProjectionToken => true,
                VoidProjectionToken => true,
                ConstantProjectionToken token => ConstantProjectionTokenEqualityComparer.Instance.Equals(token, y as ConstantProjectionToken),
                _ => throw new ArgumentOutOfRangeException(nameof(x)),
            };
        }

        public int GetHashCode(IProjectionToken obj)
        {
            return obj switch
            {
                BodyBasedProjectionToken token => BodyBasedProjectionTokenEqualityComparer.Instance.GetHashCode(token),
                MatchedInputBasedProjectionToken => typeof(MatchedInputBasedProjectionToken).GetHashCode(),
                VoidProjectionToken => typeof(VoidProjectionToken).GetHashCode(),
                ConstantProjectionToken token => ConstantProjectionTokenEqualityComparer.Instance.GetHashCode(token),
                _ => throw new ArgumentOutOfRangeException(nameof(obj)),
            };
        }
    }

    private sealed class ConstantProjectionTokenEqualityComparer : IEqualityComparer<ConstantProjectionToken>
    {
        public static readonly ConstantProjectionTokenEqualityComparer Instance = new();

        private ConstantProjectionTokenEqualityComparer()
        {
        }

        public bool Equals(ConstantProjectionToken? x, ConstantProjectionToken? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Constant!.Equals(y.Constant);
        }

        public int GetHashCode(ConstantProjectionToken obj)
        {
            return obj.Constant?.GetHashCode() ?? 0;
        }
    }

    private sealed class BodyBasedProjectionTokenEqualityComparer : IEqualityComparer<BodyBasedProjectionToken>
    {
        public static readonly BodyBasedProjectionTokenEqualityComparer Instance = new();

        private BodyBasedProjectionTokenEqualityComparer()
        {
        }

        public bool Equals(BodyBasedProjectionToken? x, BodyBasedProjectionToken? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Body == y.Body;
        }

        public int GetHashCode(BodyBasedProjectionToken obj)
        {
            return obj.Body.GetHashCode();
        }
    }
}