using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    //字段表达式
    public class FieldExp : ExpNode
    {
        public FieldExp(Token token)
            : base(token)
        {

        }
    }
}
