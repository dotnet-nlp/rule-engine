using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Resolving;

public interface ITypeResolver
{
    Type Resolve(
        ICSharpTypeToken typeDeclaration,
        IReadOnlySet<string> usingNamespaces,
        IAssembliesProvider assembliesProvider
    );
}