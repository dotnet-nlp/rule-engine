using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Reflection;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

internal interface IComposer : IUsedWordsProvider
{
    bool Match(
        string[] sequence,
        ref int index,
        in PegProcessorDataCollector dataCollector,
        RuleSpaceArguments? ruleSpaceArguments,
        IRuleSpaceCache? cache
    );
}