using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;

public sealed class AssembliesProvider : IAssembliesProvider
{
    private readonly IReadOnlyCollection<Assembly> _assemblies;
    private readonly IReadOnlyCollection<MetadataReference> _metadataReferences;

    public AssembliesProvider(IReadOnlyCollection<Assembly> assemblies, IReadOnlyCollection<MetadataReference> metadataReferences)
    {
        _assemblies = assemblies;
        _metadataReferences = metadataReferences;
    }

    public IEnumerable<Assembly> GetAssemblies()
    {
        return _assemblies;
    }

    public IEnumerable<MetadataReference> GetMetadataReferences()
    {
        return _metadataReferences;
    }
}