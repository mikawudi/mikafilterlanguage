using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    //获取字段表达式
    public class GetFieldExp : ExpNode
    {
        public ExpNode ParentNode { get; private set; }
        public ExpNode FieldNode { get; private set; }

        public GetFieldExp(Token token, ExpNode parent, ExpNode field)
            : base(token)
        {
            this.ParentNode = parent;
            this.FieldNode = field;
        }
    }
}
