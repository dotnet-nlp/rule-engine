using System.Linq;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

public sealed class ClassicCSharpTypeToken : ICSharpTypeToken
{
    public CSharpTypeNameWithNamespaceToken TypeDeclaration { get; }
    public ICSharpTypeToken[] GenericArguments { get; }

    public ClassicCSharpTypeToken(
        CSharpTypeNameWithNamespaceToken typeDeclaration,
        ICSharpTypeToken[] genericArguments
    )
    {
        TypeDeclaration = typeDeclaration;
        GenericArguments = genericArguments;
    }

    public override string ToString()
    {
        return $"{TypeDeclaration}{FormatGenericArguments()}";

        string FormatGenericArguments()
        {
            return GenericArguments.Length > 0
                ? $"<{GenericArguments.Select(type => type.ToString()).JoinToString(", ")}>"
                : "";
        }
    }
}