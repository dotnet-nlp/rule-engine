﻿using System;
using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;

// todo [realtime performance] see if we can just derive from dictionary
public sealed class CapturedVariablesParameters
{
    public IReadOnlyDictionary<string, Type> Values { get; }

    public CapturedVariablesParameters(IReadOnlyDictionary<string, Type> values)
    {
        Values = values;
    }
}