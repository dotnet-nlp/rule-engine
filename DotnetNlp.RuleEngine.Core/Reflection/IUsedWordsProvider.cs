using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Reflection;

public interface IUsedWordsProvider
{
    IEnumerable<string> GetUsedWords();
}