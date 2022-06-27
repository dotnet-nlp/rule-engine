using System.Linq;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

public sealed class LiteralSetToken : ITerminalToken
{
    public bool IsNegative { get; }
    public ILiteralSetMemberToken[] Members { get; }

    public LiteralSetToken(bool isNegative, ILiteralSetMemberToken[] members)
    {
        IsNegative = isNegative;
        Members = members;
    }

    public override string ToString()
    {
        return $"[{(IsNegative ? "^" : "")}{Members.Select(member => member.ToString()).JoinToString(" ")}]";
    }
}