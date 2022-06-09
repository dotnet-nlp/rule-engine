using System;
using System.Collections.Generic;

namespace RuleEngine.Core.Build.InputProcessing.Models;

public sealed class RuleCapturedVariables
{
    public IReadOnlyDictionary<string, Type> OwnVariables { get; }
    public IReadOnlyCollection<string> ReferencedRules { get; }

    public RuleCapturedVariables(
        IReadOnlyDictionary<string, Type> ownVariables,
        IReadOnlyCollection<string> referencedRules
    )
    {
        OwnVariables = ownVariables;
        ReferencedRules = referencedRules;
    }
}