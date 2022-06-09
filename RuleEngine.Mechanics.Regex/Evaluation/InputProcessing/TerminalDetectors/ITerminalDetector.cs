using RuleEngine.Core.Reflection;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

internal interface ITerminalDetector : IUsedWordsProvider
{
    bool WordMatches(string word, out int explicitlyMatchedSymbolsCount);
}