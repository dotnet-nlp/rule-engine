using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;

namespace RuleEngine.Mechanics.Peg.Build.Tokenization.Grammar;

public partial class PegSyntaxMatcher
{
    private readonly string? _namespace;

    private readonly CSharpSyntaxMatcher _cSharpSyntaxMatcher;

    public PegSyntaxMatcher(string? @namespace, CSharpSyntaxMatcher cSharpSyntaxMatcher)
    {
        _namespace = @namespace;
        _cSharpSyntaxMatcher = cSharpSyntaxMatcher;
    }

    public PegSyntaxMatcher(bool handleLeftRecursion, string? @namespace, CSharpSyntaxMatcher cSharpSyntaxMatcher)
        : base(handleLeftRecursion)
    {
        _namespace = @namespace;
        _cSharpSyntaxMatcher = cSharpSyntaxMatcher;
    }
}