using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Helper;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using Microsoft.CodeAnalysis;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis;

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