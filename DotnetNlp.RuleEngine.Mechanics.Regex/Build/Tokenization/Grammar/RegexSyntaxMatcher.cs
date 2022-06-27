using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Grammar;

public partial class RegexSyntaxMatcher
{
    private readonly string? _namespace;

    private readonly CSharpSyntaxMatcher _cSharpSyntaxMatcher;

    public RegexSyntaxMatcher(string? @namespace, CSharpSyntaxMatcher cSharpSyntaxMatcher)
    {
        _namespace = @namespace;
        _cSharpSyntaxMatcher = cSharpSyntaxMatcher;
    }

    public RegexSyntaxMatcher(bool handleLeftRecursion, string? @namespace, CSharpSyntaxMatcher cSharpSyntaxMatcher)
        : base(handleLeftRecursion)
    {
        _namespace = @namespace;
        _cSharpSyntaxMatcher = cSharpSyntaxMatcher;
    }
}