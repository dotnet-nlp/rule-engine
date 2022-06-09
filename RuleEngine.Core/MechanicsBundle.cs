using System;
using RuleEngine.Core.Build.InputProcessing;
using RuleEngine.Core.Build.Tokenization;

namespace RuleEngine.Core;

/// <summary>
/// This class is used to pass the configuration available input processor to rule space factory.
/// </summary>
public sealed class MechanicsBundle
{
    public string Key { get; }
    public IPatternTokenizer Tokenizer { get; }
    public IInputProcessorFactory Factory { get; }
    public Type TokenType { get; }

    public MechanicsBundle(string key, IPatternTokenizer tokenizer, IInputProcessorFactory factory, Type tokenType)
    {
        Key = key;
        Tokenizer = tokenizer;
        Factory = factory;
        TokenType = tokenType;
    }
}