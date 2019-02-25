using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    //过滤器表达式
    public class FilterExp : ExpNode
    {
        public ExpNode body = null;
        public ExpNode fieldExp = null;
        public FilterExp(ExpNode body, ExpNode fieldExp)
            : base(Token.FilterOP)
        {
            this.body = body;
            this.fieldExp = fieldExp;
        }
    }
}
