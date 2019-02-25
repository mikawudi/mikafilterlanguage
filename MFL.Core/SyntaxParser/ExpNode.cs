using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    public abstract class ExpNode
    {
        public Token BaseToken;

        public ExpNode(Token token)
        {
            this.BaseToken = token;
        }
    }
}
