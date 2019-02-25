using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFL.Core.TokenParser
{
    public class Token
    {
        public readonly string Source;
        public readonly TokenType OpType;
        public static readonly Token DotOp = new Token(".", TokenType.DotOp);
        public static readonly Token FilterStart = new Token("(", TokenType.FilterStart);
        public static readonly Token FilterEnd = new Token(")", TokenType.FilterEnd);
        public static readonly Token FilterOP = new Token("=>", TokenType.FilterOP);
        public static readonly Token EQ = new Token("eq", TokenType.EQ);
        public static readonly Token AND = new Token("and", TokenType.AND);
        public static readonly Token OR = new Token("or", TokenType.OR);
        public static readonly Token GT = new Token("gt", TokenType.GT);
        public static readonly Token GE = new Token("ge", TokenType.GE);
        public static readonly Token LL = new Token("ll", TokenType.LL);
        public static readonly Token LE = new Token("le", TokenType.LE);
        public static readonly Token WITH = new Token("with", TokenType.WITH);
        public static readonly Token LINK = new Token(",", TokenType.LINK);
        public static readonly Token BEGIN = new Token(null, TokenType.Begin);
        public static readonly Token END = new Token(null, TokenType.End);
        public Token(string source, TokenType type)
        {
            this.Source = source;
            this.OpType = type;
        }
    }
}
