using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    //常量表达式
    public class ConstExp : ExpNode
    {
        public ConstExp(Token token)
            : base(token)
        {

        }
    }
}
