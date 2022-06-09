using System;
using System.Collections;
using System.Linq;

namespace RuleEngine.Core.Lib.Common.Helpers;

public static class ErrorIndexHelper
{
    public static void FillExceptionData(IDictionary data, string source, string? error = null, int? errorIndex = null)
    {
        data["source"] = source;
        data["error"] = error;

        if (errorIndex is not null)
        {
            data["error_index"] = errorIndex.Value;

            var context = FindContext(source, errorIndex.Value);

            data["error_line_index"] = context.LineIndex;
            data["error_position_in_line"] = context.PositionInLine;

            if (context.Highlighter is not null)
            {
                data["error_character"] = context.Highlighter.ErrorCharacter;
                data["error_context"] = context.Highlighter.StringAroundError;
            }
        }
    }

    public static ErrorContext FindContext(string source, int errorIndex, int contextSize = 5)
    {
        var lines = source.ReplaceLineEndings().Split(Environment.NewLine);

        var accumulatedLength = 0;
        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];

            if (line.Length + Environment.NewLine.Length + accumulatedLength > errorIndex)
            {
                var positionInLine = errorIndex - accumulatedLength;

                ErrorHighlighter? highlighter = null;

                if (positionInLine < line.Length)
                {
                    highlighter = new ErrorHighlighter(line[positionInLine], CutLineAroundError(line, positionInLine, contextSize));
                }

                return new ErrorContext(lineIndex, positionInLine, highlighter);
            }

            accumulatedLength += line.Length + Environment.NewLine.Length;
        }

        throw new ArgumentOutOfRangeException(nameof(errorIndex), errorIndex, "Error index exceeds source length");
    }

    private static string CutLineAroundError(string line, int positionInLine, int contextSize)
    {
        return line
            .Skip(Math.Max(0, positionInLine - contextSize))
            .Take(Math.Min(positionInLine, contextSize) + 1 + contextSize)
            .JoinCharsToString();
    }

    public sealed class ErrorContext
    {
        public int LineIndex { get; }
        public int PositionInLine { get; }
        public ErrorHighlighter? Highlighter { get; }

        public ErrorContext(int lineIndex, int positionInLine, ErrorHighlighter? highlighter)
        {
            LineIndex = lineIndex;
            PositionInLine = positionInLine;
            Highlighter = highlighter;
        }
    }

    public sealed class ErrorHighlighter
    {
        public char ErrorCharacter { get; }
        public string StringAroundError { get; }

        public ErrorHighlighter(char errorCharacter, string stringAroundError)
        {
            ErrorCharacter = errorCharacter;
            StringAroundError = stringAroundError;
        }
    }
}