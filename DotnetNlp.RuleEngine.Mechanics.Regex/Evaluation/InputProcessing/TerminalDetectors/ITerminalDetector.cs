using DotnetNlp.RuleEngine.Core.Reflection;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

internal interface ITerminalDetector : IUsedWordsProvider
{
    bool WordMatches(string word, out int explicitlyMatchedSymbolsCount);
}