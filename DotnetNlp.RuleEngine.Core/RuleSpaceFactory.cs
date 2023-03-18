using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing.Models;
using DotnetNlp.RuleEngine.Core.Build.Rule.Projection;
using DotnetNlp.RuleEngine.Core.Build.Rule.Projection.Models;
using DotnetNlp.RuleEngine.Core.Build.Rule.Source;
using DotnetNlp.RuleEngine.Core.Build.Rule.Static;
using DotnetNlp.RuleEngine.Core.Build.Tokenization;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Formatting;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Resolving;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core;

/// <summary>
/// This class represents the logic of compiling rule space from its sources.
/// </summary>
public sealed class RuleSpaceFactory
{
    private readonly IProjectionCompiler _projectionCompiler;
    private readonly ITypeResolver _typeResolver;
    private readonly IReadOnlyDictionary<Type, IInputProcessorFactory> _inputProcessorFactoriesByPatternTokenType;
    private int _lastUsedCachedMatcherId = 0;
    private int _lastRuleSpaceId = 0;

    public StaticRuleFactory StaticRuleFactory { get; }
    public IReadOnlyDictionary<string, IPatternTokenizer> PatternTokenizers { get; }
    public IRuleSetTokenizer RuleSetTokenizer { get; }

    public RuleSpaceFactory(IReadOnlyCollection<MechanicsDescription> mechanicsCollection, string lineEnding = "\r\n")
    {
        _projectionCompiler = new ProjectionCompiler(
            new CachedTypeFormatter(new TypeFormatter()),
            new CodeEmitter(
                "DotnetNlp.RuleEngine.Core.Projections",
                $"{typeof(ProjectionCompiler).Namespace!}.Generated",
                "ProjectionContainer_",
                "ApplyProjection_"
            )
        );
        _typeResolver = new TypeResolver();
        _inputProcessorFactoriesByPatternTokenType = mechanicsCollection
            .Select(mechanics => new KeyValuePair<Type, IInputProcessorFactory>(mechanics.TokenType, mechanics.Factory))
            .ToDictionaryWithKnownCapacity(mechanicsCollection.Count);
        StaticRuleFactory = new StaticRuleFactory(this);
        PatternTokenizers = mechanicsCollection.ToDictionary(mechanics => mechanics.Key, mechanics => mechanics.Tokenizer);
        RuleSetTokenizer = new LoopBasedRuleSetTokenizer(PatternTokenizers, new ErrorIndexHelper(lineEnding));
    }

    /// <summary>
    /// This method allows to create a rule space from different sources:
    /// rule set, rules, already built (standalone) matchers, and rule spaces, built before.
    /// </summary>
    /// <param name="name">
    /// Rule space unique identifier (name).
    /// </param>
    /// <param name="ruleSets">
    /// Collection of rule sets to include to new rule space.
    /// The keys for the provided rules will be extracted from the rule tokens.
    /// </param>
    /// <param name="rulesByName">
    /// Collection of rules to include to new rule space.
    /// The keys for the provided rules will be extracted from the rule tokens.
    /// </param>
    /// <param name="standaloneMatchersByName">
    /// Collection of previously compiled standalone rule matchers to include to new rule space.
    /// The keys for the provided rules will be extracted from the this dictionary keys.
    /// </param>
    /// <param name="includedRuleSpaces">
    /// Collection of previously compiled rule spaces to include to new rule space. Included rule space names
    /// will be used as prefixes of the respective values' matchers in new rule space.
    /// </param>
    /// <param name="ruleSpaceParameterTypes">
    /// Description of the rule space parameters.
    /// The values of these types can be passed as respective arguments when calling rule matcher.
    /// </param>
    /// <param name="assembliesProvider">
    /// todo remove or describe
    /// </param>
    /// <param name="rootRuleName">
    /// The conventional value of rule set "root rule" name.
    /// Root rule (if exists) is invoked when the rule set is referenced "as whole" (by it's namespace).
    /// </param>
    /// <returns>Compiled rule space.</returns>
    public IRuleSpace Create(
        string name,
        IReadOnlyCollection<RuleSetToken> ruleSets,
        IReadOnlyCollection<IRuleToken> rulesByName,
        IReadOnlyDictionary<string, IRuleMatcher> standaloneMatchersByName,
        IReadOnlyCollection<IRuleSpace> includedRuleSpaces,
        IReadOnlyDictionary<string, Type> ruleSpaceParameterTypes,
        IAssembliesProvider assembliesProvider,
        string rootRuleName = "Root"
    )
    {
        IReadOnlyDictionary<string, IRuleToken> ruleTokensByName = CreateRuleTokens(ruleSets, rulesByName);

        IReadOnlyDictionary<string, IRuleMatcher> transientMatchersByName = Enumerable
            .Empty<IEnumerable<KeyValuePair<string, IRuleMatcher>>>()
            .Append(standaloneMatchersByName)
            .Append(
                includedRuleSpaces
                    .SelectMany(
                        includedRuleSpace => includedRuleSpace
                            .GetNonTransientRules()
                            .MapKey(key => $"{includedRuleSpace.Name}.{key}")
                    )
            )
            .MergeWithKnownCapacity(
                standaloneMatchersByName.Count + includedRuleSpaces.Aggregate(0, (count, ruleSpace) => count + ruleSpace.RuleResultTypesByName.Count),
                true
            );

        var aliases = CreateAliases(ruleTokensByName, rootRuleName);

        var usingsByRuleName = GetUsingsByRuleName(ruleSets, aliases);

        var ruleSpaceDescription = new RuleSpaceDescription(
            _typeResolver,
            assembliesProvider,
            ruleTokensByName,
            transientMatchersByName,
            aliases,
            usingsByRuleName
        );

        var capturedVariablesParametersByRuleName = GetCapturedVariablesParametersByRuleName(ruleTokensByName, transientMatchersByName, ruleSpaceDescription, aliases);
        var ruleParametersByRuleName = GetRuleParametersByRuleName(ruleTokensByName, usingsByRuleName, assembliesProvider, aliases);

        var projectionsByRuleName = CreateProjections(
            ruleSpaceDescription,
            assembliesProvider,
            ruleTokensByName,
            usingsByRuleName,
            capturedVariablesParametersByRuleName,
            ruleParametersByRuleName,
            ruleSpaceParameterTypes,
            aliases
        );

        var ruleSources = Enumerable
            .Empty<KeyValuePair<string, IRuleSource>>()
            .Concat(CreateTokenBasedRuleSources(ruleTokensByName, capturedVariablesParametersByRuleName, ruleParametersByRuleName, ruleSpaceDescription, projectionsByRuleName))
            .Concat(CreateMatcherBasedRuleSource(transientMatchersByName))
            .ToDictionaryWithKnownCapacity(ruleTokensByName.Count + transientMatchersByName.Count);

        var ruleSpaceBuilder = new RuleSpaceBuilder(
            name,
            ruleSpaceDescription,
            ruleSpaceParameterTypes,
            ruleSources,
            aliases,
            this,
            transientMatchersByName.Keys.ToHashSet()
        );

        return ruleSpaceBuilder.Build(++_lastRuleSpaceId);
    }

    public IRuleMatcher AddRule(
        IRuleSpace ruleSpace,
        IRuleToken rule,
        IAssembliesProvider assembliesProvider
    )
    {
        var ruleKey = rule.Name;

        var subscriptions = new List<Action>();

        var ruleMatcher = CreateSingleRule();

        ruleSpace[ruleKey] = ruleMatcher;

        foreach (var subscription in subscriptions)
        {
            subscription();
        }

        return ruleMatcher;

        IRuleMatcher CreateSingleRule()
        {
            var resultType = _typeResolver
                .Resolve(rule.ReturnType, ImmutableHashSet<string>.Empty, assembliesProvider);

            var ruleParameters = GetRuleParameters(
                rule,
                assembliesProvider,
                ImmutableHashSet<string>.Empty
            );

            var description = new RuleSpaceBasedRuleSpaceDescription(ruleSpace);

            var ownCapturedVariables = CollectOwnCapturedVariables(rule, description);

            var capturedVariablesParameters = new CapturedVariablesParameters(
                YieldAllCapturedVariables(ownCapturedVariables).ToDictionary(true)
            );

            var ruleSource = CreateRuleSource(
                rule,
                ruleParameters,
                capturedVariablesParameters,
                resultType,
                _projectionCompiler
                    .CreateProjection(
                        ruleKey,
                        CreateProjectionCompilationData(
                            ruleKey,
                            rule,
                            ImmutableHashSet<string>.Empty,
                            resultType,
                            capturedVariablesParameters,
                            ruleParameters,
                            new RuleSpaceParameters(ruleSpace.RuleSpaceParameterTypesByName)
                        ),
                        assembliesProvider
                    ),
                description
            );

            return WrapWithCache(ruleSource.GetRuleMatcher(ruleSpace, subscriptions.Add));

            IEnumerable<KeyValuePair<string, Type>> YieldAllCapturedVariables(RuleCapturedVariables capturedVariables)
            {
                foreach (var ownVariable in capturedVariables.OwnVariables)
                {
                    yield return ownVariable;
                }

                foreach (var referencedRuleKey in capturedVariables.ReferencedRules)
                {
                    if (ruleSpace.TryGetValue(referencedRuleKey, out var referencedMatcher))
                    {
                        foreach (var referencedVariable in referencedMatcher.ResultDescription.CapturedVariablesTypes)
                        {
                            yield return referencedVariable;
                        }
                    }
                    else
                    {
                        throw new RuleBuildException(
                            $"Rule '{referencedMatcher}' referenced from rule '{ruleKey}' doesn't exist."
                        );
                    }
                }
            }
        }
    }

    internal CachingRuleMatcher WrapWithCache(IRuleMatcher source)
    {
        return new CachingRuleMatcher(++_lastUsedCachedMatcherId, source);
    }

    private IReadOnlyDictionary<string, CapturedVariablesParameters> GetCapturedVariablesParametersByRuleName(
        IReadOnlyDictionary<string, IRuleToken> ruleTokensByName,
        IReadOnlyDictionary<string, IRuleMatcher> transientMatchersByName,
        IRuleSpaceDescription ruleSpaceDescription,
        IReadOnlyDictionary<string, string> aliases
    )
    {
        var capturedVariablesByRuleKey = new Dictionary<string, RuleCapturedVariables>(
            ruleTokensByName.Count
        );

        // the process is split into two parts intentionally:
        // first we collect all own variables (i.e. declared directly in the rule)
        foreach (var (ruleKey, ruleToken) in ruleTokensByName)
        {
            capturedVariablesByRuleKey.Add(
                ruleKey,
                CollectOwnCapturedVariables(ruleToken, ruleSpaceDescription)
            );
        }

        // second we collect all the variables (including variables from referenced rules)
        return capturedVariablesByRuleKey
            .MapValue(
                (ruleKey, capturedVariables) => new CapturedVariablesParameters(
                    YieldAllCapturedVariables(
                            ruleKey,
                            capturedVariables,
                            new HashSet<string>
                            {
                                ruleKey,
                            }
                        )
                        .ToDictionary(true)
                )
            )
            .ToDictionaryWithKnownCapacity(ruleTokensByName.Count + aliases.Count)
            .AddAliases(aliases);

        IEnumerable<KeyValuePair<string, Type>> CollectCapturedVariables(
            string originalRuleKey,
            string referencedRuleKey,
            in ISet<string> discoveredReferences
        )
        {
            if (discoveredReferences.Contains(referencedRuleKey))
            {
                return Enumerable.Empty<KeyValuePair<string, Type>>();
            }

            discoveredReferences.Add(referencedRuleKey);

            if (capturedVariablesByRuleKey.TryGetValue(referencedRuleKey, out var capturedVariables))
            {
                return YieldAllCapturedVariables(originalRuleKey, capturedVariables, discoveredReferences);
            }

            return transientMatchersByName.TryGetValue(referencedRuleKey, out var ruleMatcher)
                ? ruleMatcher.ResultDescription.CapturedVariablesTypes
                : Enumerable.Empty<KeyValuePair<string, Type>>();
        }

        IEnumerable<KeyValuePair<string, Type>> YieldAllCapturedVariables(
            string originalRuleKey,
            RuleCapturedVariables capturedVariables,
            ISet<string> discoveredReferences
        )
        {
            foreach (var ownVariable in capturedVariables.OwnVariables)
            {
                yield return ownVariable;
            }

            foreach (var referencedRuleKey in capturedVariables.ReferencedRules)
            {
                foreach (var referencedVariable in CollectCapturedVariables(originalRuleKey, referencedRuleKey, discoveredReferences))
                {
                    yield return referencedVariable;
                }
            }
        }
    }

    private RuleCapturedVariables CollectOwnCapturedVariables(
        IRuleToken ruleToken,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        return ruleToken switch
        {
            EmptyRuleToken => new RuleCapturedVariables(ImmutableDictionary<string, Type>.Empty, Array.Empty<string>()),
            RuleToken rule => ExtractOwnCapturedVariables(rule),
            _ => throw new ArgumentOutOfRangeException(nameof(ruleToken)),
        };

        RuleCapturedVariables ExtractOwnCapturedVariables(RuleToken rule)
        {
            return ExtractVariables(rule.Pattern, ((IRuleToken) rule).GetFullName());
        }

        RuleCapturedVariables ExtractVariables(IPatternToken pattern, string ruleName)
        {
            var key = pattern.GetType();

            if (!_inputProcessorFactoriesByPatternTokenType.TryGetValue(key, out var factory))
            {
                throw new RuleBuildException(
                    $"Input processor for pattern token '{key.FullName}' does not exist."
                );
            }

            try
            {
                return factory.ExtractOwnCapturedVariables(pattern, ruleSpaceDescription);
            }
            catch (Exception exception)
            {
                throw new RuleBuildException(
                    $"Cannot get variables from rule '{ruleName}'.",
                    exception
                );
            }
        }
    }

    private IReadOnlyDictionary<string, RuleParameters> GetRuleParametersByRuleName(
        IReadOnlyDictionary<string, IRuleToken> ruleTokensByName,
        IReadOnlyDictionary<string, IReadOnlySet<string>> usingsByRuleName,
        IAssembliesProvider assembliesProvider,
        IReadOnlyDictionary<string, string> aliases
    )
    {
        return ruleTokensByName
            .MapValue(
                (ruleKey, rule) => GetRuleParameters(
                    rule,
                    assembliesProvider,
                    usingsByRuleName.GetValueOrDefault(ruleKey, ImmutableHashSet<string>.Empty)
                )
            )
            .ToDictionaryWithKnownCapacity(ruleTokensByName.Count + aliases.Count)
            .AddAliases(aliases);
    }

    private RuleParameters GetRuleParameters(
        IRuleToken rule,
        IAssembliesProvider assembliesProvider,
        IReadOnlySet<string> usings
    )
    {
        return new RuleParameters(
            rule
                .RuleParameters
                .Select(
                    ruleParameter => new KeyValuePair<string, Type>(
                        ruleParameter.Name,
                        _typeResolver.Resolve(ruleParameter.Type, usings, assembliesProvider)
                    )
                )
                .ToArray()
        );
    }

    private static IEnumerable<KeyValuePair<string, IRuleSource>> CreateMatcherBasedRuleSource(
        IReadOnlyDictionary<string, IRuleMatcher> matcherBasedRulesByName
    )
    {
        return matcherBasedRulesByName.MapValue(matcher => (IRuleSource) new MatcherBasedRuleSource(matcher));
    }

    private IEnumerable<KeyValuePair<string, IRuleSource>> CreateTokenBasedRuleSources(
        IReadOnlyDictionary<string, IRuleToken> ruleTokensByName,
        IReadOnlyDictionary<string, CapturedVariablesParameters> capturedVariablesParametersByRuleName,
        IReadOnlyDictionary<string, RuleParameters> ruleParametersByRuleName,
        IRuleSpaceDescription ruleSpaceDescription,
        IReadOnlyDictionary<string, IRuleProjection> projectionsByRuleName
    )
    {
        return ruleTokensByName
            .MapValue(
                (ruleKey, ruleToken) => CreateRuleSource(
                    ruleToken,
                    ruleParametersByRuleName[ruleKey],
                    capturedVariablesParametersByRuleName[ruleKey],
                    ruleSpaceDescription[ruleKey],
                    projectionsByRuleName[ruleKey],
                    ruleSpaceDescription
                )
            );
    }

    private IRuleSource CreateRuleSource(
        IRuleToken ruleToken,
        RuleParameters ruleParameters,
        CapturedVariablesParameters capturedVariablesParameters,
        Type resultType,
        IRuleProjection projection,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        return ruleToken switch
        {
            EmptyRuleToken emptyRule => new EmptyRuleTokenBasedRuleSource(
                emptyRule,
                ruleParameters,
                new RuleMatchResultDescription(resultType, capturedVariablesParameters.Values)
            ),
            RuleToken rule => new RuleTokenBasedRuleSource(
                rule,
                _inputProcessorFactoriesByPatternTokenType[rule.Pattern.GetType()],
                ruleParameters,
                capturedVariablesParameters,
                new RuleMatchResultDescription(resultType, capturedVariablesParameters.Values),
                projection,
                ruleSpaceDescription
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(ruleToken)),
        };
    }

    private IReadOnlyDictionary<string, IRuleProjection> CreateProjections(
        IRuleSpaceDescription ruleSpaceDescription,
        IAssembliesProvider assembliesProvider,
        IReadOnlyDictionary<string, IRuleToken> ruleTokens,
        IReadOnlyDictionary<string, IReadOnlySet<string>> usingsByRuleName,
        IReadOnlyDictionary<string, CapturedVariablesParameters> capturedVariablesParametersByRuleName,
        IReadOnlyDictionary<string, RuleParameters> ruleParametersByRuleName,
        IReadOnlyDictionary<string, Type> ruleSpaceParametersTypes,
        IReadOnlyDictionary<string, string> aliases
    )
    {
        return _projectionCompiler
            .CreateProjections(
                ruleTokens
                    .MapValue(
                        (ruleKey, rule) => CreateProjectionCompilationData(
                            ruleKey,
                            rule,
                            usingsByRuleName.GetValueOrDefault(ruleKey, ImmutableHashSet<string>.Empty),
                            ruleSpaceDescription[ruleKey],
                            capturedVariablesParametersByRuleName[ruleKey],
                            ruleParametersByRuleName[ruleKey],
                            new RuleSpaceParameters(ruleSpaceParametersTypes)
                        )
                    )
                    .ToDictionaryWithKnownCapacity(ruleTokens.Count),
                assembliesProvider,
                aliases.Count
            )
            .AddAliases(aliases);
    }

    private static IProjectionCompilationData CreateProjectionCompilationData(
        string ruleKey,
        IRuleToken rule,
        IReadOnlySet<string> usings,
        Type resultType,
        CapturedVariablesParameters capturedVariablesParameters,
        RuleParameters ruleParameters,
        RuleSpaceParameters ruleSpaceParameters
    )
    {
        return rule.Projection switch
        {
            BodyBasedProjectionToken bodyBasedProjection => new BodyBasedProjectionCompilationData(
                usings,
                resultType,
                new ProjectionParameters(
                    typeof(string[]),
                    capturedVariablesParameters,
                    ruleParameters,
                    ruleSpaceParameters
                ),
                bodyBasedProjection.Body
            ),
            MatchedInputBasedProjectionToken => MatchedInputBasedProjectionCompilationData.Instance,
            VoidProjectionToken => VoidProjectionCompilationData.Instance,
            ConstantProjectionToken constantProjection => new ConstantProjectionCompilationData(
                constantProjection.Constant
            ),
            _ => throw new RuleBuildException(
                $"Rule {ruleKey} has unknown projection type {rule.Projection.GetType().FullName}"
            ),
        };
    }

    private static Dictionary<string, IRuleToken> CreateRuleTokens(
        IReadOnlyCollection<RuleSetToken> ruleSets,
        IReadOnlyCollection<IRuleToken> rulesByName
    )
    {
        var capacity = rulesByName.Count + ruleSets.Aggregate(0, (count, ruleSet) => count + ruleSet.Rules.Length);

        return ruleSets
            .SelectMany(ruleSet => ruleSet.Rules)
            .Concat(rulesByName)
            .Select(rule => new KeyValuePair<string, IRuleToken>(rule.GetFullName(), rule))
            .ToDictionaryWithKnownCapacity(
                capacity,
                false,
                (ruleKey, argumentException) => new RuleBuildException($"Duplicate rule '{ruleKey}'.", argumentException)
            );
    }

    private static IReadOnlyDictionary<string, string> CreateAliases(
        IReadOnlyDictionary<string, IRuleToken> ruleTokensByName,
        string rootRuleName
    )
    {
        bool Filter(IRuleToken rule) => rule.Namespace is not null && rule.Name == rootRuleName;

        var capacity = ruleTokensByName.Aggregate(0, (count, pair) => Filter(pair.Value) ? ++count : count);

        return ruleTokensByName
            .WhereValue(Filter)
            .MapValue(rule => rule.Namespace!)
            .SwapKeysAndValues()
            .ToDictionaryWithKnownCapacity(capacity);
    }

    private static IReadOnlyDictionary<string, IReadOnlySet<string>> GetUsingsByRuleName(
        IReadOnlyCollection<RuleSetToken> ruleSets,
        IReadOnlyDictionary<string, string> aliases
    )
    {
        var capacity = aliases.Count + ruleSets.Aggregate(0, (count, ruleSet) => count + ruleSet.Rules.Length);

        return ruleSets
            .SelectMany(
                ruleSet =>
                {
                    IReadOnlySet<string> usings = ruleSet.Usings.Select(@using => @using.Namespace).ToHashSet();

                    return ruleSet
                        .Rules
                        .Select(
                            rule => new KeyValuePair<string, IReadOnlySet<string>>(
                                ((IRuleToken) rule).GetFullName(),
                                usings
                            )
                        );
                }
            )
            .ToDictionaryWithKnownCapacity(capacity)
            .AddAliases(aliases);
    }

    private sealed class RuleSpaceDescription : IRuleSpaceDescription
    {
        private readonly ITypeResolver _typeResolver;
        private readonly IAssembliesProvider _assembliesProvider;

        private readonly IReadOnlyDictionary<string, IRuleToken> _tokenBasedRules;
        private readonly IReadOnlyDictionary<string, IRuleMatcher> _matcherBasedRules;
        private readonly IReadOnlyDictionary<string, string> _aliases;
        private readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _usingsByRuleName;

        private Dictionary<string, Type>? _returnTypesByRuleName;

        public IReadOnlyDictionary<string, Type> ResultTypesByRuleName
        {
            get
            {
                return _returnTypesByRuleName ??= new[]
                    {
                        _tokenBasedRules
                            .MapValue(
                                (ruleKey, ruleToken) => _typeResolver.Resolve(
                                    ruleToken.ReturnType,
                                    _usingsByRuleName
                                        .GetValueOrDefault(ruleKey, ImmutableHashSet<string>.Empty),
                                    _assembliesProvider
                                )
                            ),
                        _matcherBasedRules.MapValue(matcher => matcher.ResultDescription.ResultType),
                    }
                    .MergeWithKnownCapacity(
                        _tokenBasedRules.Count + _matcherBasedRules.Count + _aliases.Count
                    )
                    .AddAliases(_aliases);
            }
        }

        public Type this[string ruleKey]
        {
            get
            {
                if (ResultTypesByRuleName.TryGetValue(ruleKey, out var ruleResultType))
                {
                    return ruleResultType;
                }

                throw new RuleBuildException($"Description for rule {ruleKey} not found.");
            }
        }

        public RuleSpaceDescription(
            ITypeResolver typeResolver,
            IAssembliesProvider assembliesProvider,
            IReadOnlyDictionary<string, IRuleToken> tokenBasedRules,
            IReadOnlyDictionary<string, IRuleMatcher> matcherBasedRules,
            IReadOnlyDictionary<string, string> aliases,
            IReadOnlyDictionary<string, IReadOnlySet<string>> usingsByRuleName
        )
        {
            _typeResolver = typeResolver;
            _assembliesProvider = assembliesProvider;
            _tokenBasedRules = tokenBasedRules;
            _matcherBasedRules = matcherBasedRules;
            _aliases = aliases;
            _usingsByRuleName = usingsByRuleName;
        }
    }
}