using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    //二元表达式
    public class BinOpExp : ExpNode
    {
        public ExpNode Left;
        public ExpNode Right;
        public BinOpExp(Token token, ExpNode left, ExpNode right)
            : base(token)
        {
            this.Left = left;
            this.Right = right;
        }
    }
}
