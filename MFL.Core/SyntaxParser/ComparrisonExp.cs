using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    //对比表达式
    public class ComparrisonExp : ExpNode
    {
        public ExpNode LeftNode { get; private set; }
        public ExpNode RightNode { get; private set; }
        public ComparrisonExp(Token token, ExpNode left, ExpNode right)
            : base(token)
        {
            this.LeftNode = left;
            this.RightNode = right;
        }
    }
}
