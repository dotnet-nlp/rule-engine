using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Reflection;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

internal interface IComposer : IUsedWordsProvider
{
    bool Match(
        RuleInput input,
        ref int index,
        in PegInputProcessorDataCollector dataCollector,
        IRuleSpaceCache cache
    );
}