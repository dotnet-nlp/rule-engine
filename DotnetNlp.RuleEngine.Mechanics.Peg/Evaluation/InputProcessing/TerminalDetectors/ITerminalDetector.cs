using DotnetNlp.RuleEngine.Core.Reflection;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

internal interface ITerminalDetector : IUsedWordsProvider
{
    bool WordMatches(string word);
}