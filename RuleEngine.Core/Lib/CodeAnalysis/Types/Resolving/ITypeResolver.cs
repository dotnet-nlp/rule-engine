using System;
using System.Collections.Generic;
using RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace RuleEngine.Core.Lib.CodeAnalysis.Types.Resolving;

public interface ITypeResolver
{
    Type Resolve(
        ICSharpTypeToken typeDeclaration,
        IReadOnlySet<string> usingNamespaces,
        IAssembliesProvider assembliesProvider
    );
}