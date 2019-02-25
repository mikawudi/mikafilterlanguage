using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFL.Core.TokenParser
{
    public class DoubleToken : Token
    {
        public readonly double number;

        public DoubleToken(string source)
            : base(source, TokenType.Double)
        {
            number = double.Parse(source);
        }
    }
}
