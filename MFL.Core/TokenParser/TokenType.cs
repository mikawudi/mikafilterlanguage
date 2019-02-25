using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFL.Core.TokenParser
{
    public enum TokenType
    {
        Field,
        DotOp,
        FilterStart,
        FilterEnd,
        FilterOP,
        EQ,
        AND,
        OR,
        GT,
        GE,
        LL,
        LE,
        ConstStr,
        Int,
        Double,
        Begin,
        End,
        WITH,
        LINK
    }
}
