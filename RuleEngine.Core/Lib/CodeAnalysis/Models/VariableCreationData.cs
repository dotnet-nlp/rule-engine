namespace RuleEngine.Core.Lib.CodeAnalysis.Models;

public sealed class VariableCreationData
{
    public string Name { get; }
    public string TypeDeclaration { get; }

    public VariableCreationData(string name, string typeDeclaration)
    {
        Name = name;
        TypeDeclaration = typeDeclaration;
    }
}