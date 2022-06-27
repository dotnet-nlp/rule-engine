using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Grammar;

public partial class RuleSetSyntaxMatcher
{
    private readonly string? _namespace;
    private readonly IReadOnlyDictionary<string, IPatternTokenizer> _patternParsers;
    private readonly CSharpSyntaxMatcher _cSharpSyntaxMatcher;
    private readonly bool _caseSensitive;

    public RuleSetSyntaxMatcher(
        string? @namespace,
        IReadOnlyDictionary<string, IPatternTokenizer> patternParsers,
        CSharpSyntaxMatcher cSharpSyntaxMatcher,
        bool caseSensitive
    )
    {
        _namespace = @namespace;
        _patternParsers = patternParsers;
        _cSharpSyntaxMatcher = cSharpSyntaxMatcher;
        _caseSensitive = caseSensitive;
    }

    public RuleSetSyntaxMatcher(
        bool handleLeftRecursion,
        string? @namespace,
        IReadOnlyDictionary<string, IPatternTokenizer> patternParsers,
        CSharpSyntaxMatcher cSharpSyntaxMatcher,
        bool caseSensitive
    )
        : base(handleLeftRecursion)
    {
        _namespace = @namespace;
        _patternParsers = patternParsers;
        _cSharpSyntaxMatcher = cSharpSyntaxMatcher;
        _caseSensitive = caseSensitive;
    }
}