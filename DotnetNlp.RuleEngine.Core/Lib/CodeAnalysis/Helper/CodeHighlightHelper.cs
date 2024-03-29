﻿using System;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using Microsoft.CodeAnalysis;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Helper;

public static class CodeHighlightHelper
{
    public static string DiagnosticToString(Diagnostic diagnostic, string code)
    {
        return HighlightLineWithError(
            code,
            diagnostic.Location.GetLineSpan().StartLinePosition.Line,
            diagnostic.GetMessage()
        );
    }

    private static string HighlightLineWithError(string code, int lineIndex, string message, int linesRange = 5)
    {
        var lines = code.ReplaceLineEndings().Split(Environment.NewLine);

        if (lineIndex < lines.Length)
        {
            lines[lineIndex] += "    <<<<<<<<<<<<<< " + message;
        }

        return lines
            .Skip(Math.Max(0, lineIndex - linesRange))
            .Take(Math.Min(lineIndex, linesRange) + 1 + linesRange)
            .JoinToString(Environment.NewLine);
    }
}