using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using RuleEngine.Core.Lib.CodeAnalysis.Helper;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Lib.CodeAnalysis;

public sealed class CodeEmitException : Exception
{
    public CodeEmitException(string message, string code, IEnumerable<Diagnostic> diagnostics) : base(message)
    {
        Data["code"] = code;
        Data["code_sample"] = diagnostics
            .Select(diagnostic => CodeHighlightHelper.DiagnosticToString(diagnostic, code))
            .JoinToString("\r\n");
    }
}