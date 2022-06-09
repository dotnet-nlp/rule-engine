using RuleEngine.Core.Reflection;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

internal interface ITerminalDetector : IUsedWordsProvider
{
    bool WordMatches(string word);
}