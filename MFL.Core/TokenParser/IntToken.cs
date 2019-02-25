using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFL.Core.TokenParser
{
    public class IntToken : Token
    {
        public readonly int number;
        public IntToken(string source)
            : base(source, TokenType.Int)
        {
            number = int.Parse(source);
        }
    }
}
