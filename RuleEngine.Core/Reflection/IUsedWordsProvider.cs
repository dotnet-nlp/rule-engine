using System.Collections.Generic;

namespace RuleEngine.Core.Reflection;

public interface IUsedWordsProvider
{
    IEnumerable<string> GetUsedWords();
}