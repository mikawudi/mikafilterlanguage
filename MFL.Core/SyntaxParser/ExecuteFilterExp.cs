using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    //执行过滤器表达式
    public class ExecuteFilterExp : ExpNode
    {
        public GetFieldExp GetfieldExp;
        public FilterExp FilterExp;
        public ExecuteFilterExp(GetFieldExp getfieldExp, FilterExp exp)
            : base(Token.FilterOP)
        {
            this.GetfieldExp = getfieldExp;
            this.FilterExp = exp;
        }
    }
}
