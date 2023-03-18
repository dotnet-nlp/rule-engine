using DotnetNlp.RuleEngine.Core.Reflection;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

// todo get rid of token instances in all the implementations
internal interface ITerminalDetector : IUsedWordsProvider
{
    bool WordMatches(string word);
}