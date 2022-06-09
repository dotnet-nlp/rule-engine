﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace RuleEngine.Core.Lib.CodeAnalysis.Assemblies;

public sealed class LoadedAssembliesProvider : IAssembliesProvider
{
    public IEnumerable<Assembly> GetAssemblies()
    {
        return GetAssembliesData().Select(data => data.Assembly);
    }

    public IEnumerable<MetadataReference> GetMetadataReferences()
    {
        return GetAssembliesData().Select(data => data.Reference);
    }

    private static IEnumerable<(Assembly Assembly, MetadataReference Reference)> GetAssembliesData()
    {
        return AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && assembly.Location != string.Empty)
            .Select(assembly => (assembly, (MetadataReference) MetadataReference.CreateFromFile(assembly.Location)));
    }
}