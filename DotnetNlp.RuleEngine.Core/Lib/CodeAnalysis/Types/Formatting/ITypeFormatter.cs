using System;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Formatting;

public interface ITypeFormatter
{
    string GetStringRepresentation(Type type);
}