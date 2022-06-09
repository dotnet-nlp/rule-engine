namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

internal interface IRuleReferencePayload : ITransitionPayload
{
    string RuleSpaceKey { get; }
}