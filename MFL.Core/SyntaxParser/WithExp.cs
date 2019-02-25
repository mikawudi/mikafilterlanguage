using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    //with表达式
    public class WithExp : ExpNode
    {
        public ExpNode LogicOrComExp { get; private set; }
        public List<ExecuteFilterExp> ExeFilter { get; private set; }

        public WithExp(ExpNode logicOrComExp, ExecuteFilterExp withFilter)
            : base(Token.WITH)
        {
            this.LogicOrComExp = logicOrComExp;
            this.ExeFilter = new List<ExecuteFilterExp>();
            this.ExeFilter.Add(withFilter);
        }

        public void AddWith(ExecuteFilterExp withFilter)
        {
            this.ExeFilter.Add(withFilter);
        }
    }
}
