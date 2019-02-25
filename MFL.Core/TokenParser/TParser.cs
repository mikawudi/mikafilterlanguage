using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;

namespace MFL.Core.TokenParser
{
    public static class TParser
    {

        public static List<Token> GetTokens(this string filter)
        {
            List<Token> result = new List<Token>();
            char[] buffer = new char[1024];
            int index = 0;
            //0:start 1:field 2:int 3:int. 4:double 5:= 6:string
            int state = 0;
            for (int i = 0; i < filter.Length; i++)
            {
                //index++;
                char b = filter[i];


                if ((b >= 'a' && b <= 'z') || b >= 'A' && b <= 'Z' || b == '_')
                {
                    if (state == 0)
                    {
                        buffer[index++] = b;
                        state = 1;
                        continue;
                    }

                    if (state == 1)
                    {
                        buffer[index++] = b;
                        continue;
                    }

                    if (state == 2 || state == 3 || state == 4 || state == 5)
                    {
                        throw new Exception();
                    }

                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                }

                if (b == '-')
                {
                    if (state == 0)
                    {
                        buffer[index++] = b;
                        state = 2;
                        continue;
                    }
                    throw new Exception();
                }

                if (b >= '0' && b <= '9')
                {
                    if (state == 0)
                    {
                        buffer[index++] = b;
                        state = 2;
                        continue;
                    }

                    if (state == 1)
                    {
                        buffer[index++] = b;
                        continue;
                    }

                    if (state == 2)
                    {
                        buffer[index++] = b;
                        continue;
                    }

                    if (state == 3 || state == 4)
                    {
                        buffer[index++] = b;
                        state = 4;
                        continue;
                    }

                    if (state == 5)
                    {
                        throw new Exception();
                    }

                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                }

                if (b == ' ')
                {
                    if (state == 0)
                    {
                        continue;
                    }

                    if (state == 1)
                    {
                        //todo create a token
                        result.Add(CreateToken(buffer, index, TokenType.Field));
                        index = 0;
                        state = 0;
                        continue;
                    }

                    if (state == 2)
                    {
                        //todo create int
                        result.Add(CreateToken(buffer, index, TokenType.Int));
                        index = 0;
                        state = 0;
                        continue;
                    }

                    if (state == 3)
                    {
                        throw new Exception();
                    }

                    if (state == 4)
                    {
                        //todo create double
                        result.Add(CreateToken(buffer, index, TokenType.Double));
                        index = 0;
                        state = 0;
                        continue;
                    }

                    if (state == 5)
                    {
                        throw new Exception();
                    }

                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                }

                if (b == '.')
                {
                    if (state == 0)
                    {
                        throw new Exception();
                    }

                    if (state == 1)
                    {
                        //todo create a field
                        //todo create a dotOP
                        result.Add(CreateToken(buffer, index, TokenType.Field));
                        result.Add(Token.DotOp);

                        index = 0;
                        state = 0;
                        continue;
                    }

                    if (state == 2)
                    {
                        buffer[index++] = b;
                        state = 3;
                        continue;
                    }

                    if (state == 3 || state == 4 || state == 5)
                    {
                        throw new Exception();
                    }

                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                }

                if (b == '=')
                {
                    if (state == 0)
                    {
                        buffer[index++] = b;
                        state = 5;
                        continue;
                    }

                    if (state == 1)
                    {
                        //todo create field
                        result.Add(CreateToken(buffer, index, TokenType.Field));
                        index = 0;
                        buffer[index++] = b;
                        state = 5;
                        continue;
                    }

                    if (state == 2)
                    {
                        //todo create int
                        result.Add(CreateToken(buffer, index, TokenType.Int));
                        index = 0;
                        buffer[index++] = b;
                        state = 5;
                        continue;
                    }

                    if (state == 3)
                    {
                        throw new Exception();
                    }

                    if (state == 4)
                    {
                        //todo create double
                        result.Add(CreateToken(buffer, index, TokenType.Double));
                        index = 0;
                        buffer[index++] = b;
                        state = 5;
                        continue;
                    }

                    if (state == 5)
                    {
                        throw new Exception();
                    }

                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                }

                if (b == '>')
                {
                    if (state == 0 || state == 1 || state == 2 || state == 3 || state == 4)
                    {
                        throw new Exception();
                    }

                    if (state == 5)
                    {
                        //todo create filterOP
                        result.Add(Token.FilterOP);
                        buffer[index++] = b;
                        state = 0;
                        index = 0;
                        continue;
                    }

                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                }

                if (b == '(')
                {
                    if (state == 0)
                    {
                        //todo create (
                        result.Add(Token.FilterStart);
                        state = 0;
                        index = 0;
                        continue;
                    }

                    if (state == 1)
                    {
                        //todo create field
                        result.Add(CreateToken(buffer, index, TokenType.Field));
                        //todo create (
                        result.Add(Token.FilterStart);
                        state = 0;
                        index = 0;
                        continue;
                    }

                    if (state == 2 || state == 3 || state == 4 || state == 5)
                    {
                        throw new Exception();
                    }

                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                }

                if (b == ')')
                {
                    if (state == 0)
                    {
                        //todo create )
                        result.Add(Token.FilterEnd);
                        state = 0;
                        index = 0;
                        continue;
                    }

                    if (state == 1)
                    {
                        //todo create field
                        //todo create )
                        result.Add(CreateToken(buffer, index, TokenType.Field));
                        result.Add(Token.FilterEnd);
                        state = 0;
                        index = 0;
                        continue;
                    }

                    if (state == 2)
                    {
                        //todo create int
                        //todo create )
                        result.Add(CreateToken(buffer, index, TokenType.Int));
                        result.Add(Token.FilterEnd);
                        state = 0;
                        index = 0;
                        continue;
                    }

                    if (state == 3)
                    {
                        throw new Exception();
                    }

                    if (state == 4)
                    {
                        //todo create double
                        //todo create )
                        result.Add(CreateToken(buffer, index, TokenType.Double));
                        result.Add(Token.FilterEnd);
                        state = 0;
                        index = 0;
                        continue;
                    }

                    if (state == 5)
                    {
                        throw new Exception();
                    }

                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                }

                if (b == '"')
                {
                    if (state == 0)
                    {
                        state = 6;
                        index = 0;
                        continue;
                    }
                    else if (state == 6)
                    {
                        //todo create string
                        result.Add(CreateToken(buffer, index, TokenType.ConstStr));
                        state = 0;
                        index = 0;
                        continue;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                if (b == ',')
                {
                    if (state == 6)
                    {
                        buffer[index++] = b;
                        continue;
                    }
                    if(state != 0)
                        throw new Exception();
                    buffer[0] = ',';
                    result.Add(CreateToken(buffer, 1, TokenType.Field));
                    index = 0;
                    state = 0;
                    continue;
                }


                //字符串拥有最高优先级
                if (state == 6)
                {
                    buffer[index++] = b;
                    continue;
                }

                throw new Exception();
            }

            return result;
        }


        private static Token CreateToken(char[] buffer, int length, TokenType type)
        {
            //string source = Encoding.ASCII.GetString(buffer, 0, length);
            var source = new string(buffer, 0, length);
            if (type == TokenType.Field)
            {
                if (source.Equals("eq", StringComparison.CurrentCultureIgnoreCase))
                    return Token.EQ;
                if (source.Equals("and", StringComparison.CurrentCultureIgnoreCase))
                    return Token.AND;
                if (source.Equals("or", StringComparison.CurrentCultureIgnoreCase))
                    return Token.OR;
                if (source.Equals("gt", StringComparison.CurrentCultureIgnoreCase))
                    return Token.GT;
                if (source.Equals("ge", StringComparison.CurrentCultureIgnoreCase))
                    return Token.GE;
                if (source.Equals("ll", StringComparison.CurrentCultureIgnoreCase))
                    return Token.LL;
                if (source.Equals("le", StringComparison.CurrentCultureIgnoreCase))
                    return Token.LE;
                if (source.Equals("with", StringComparison.CurrentCultureIgnoreCase))
                    return Token.WITH;
                if (source.Equals(",", StringComparison.CurrentCultureIgnoreCase))
                    return Token.LINK;
            }

            if (type == TokenType.DotOp)
                return Token.DotOp;
            if (type == TokenType.FilterStart)
                return Token.FilterStart;
            if (type == TokenType.FilterEnd)
                return Token.FilterEnd;
            if (type == TokenType.FilterOP)
                return Token.FilterOP;
            if (type == TokenType.Int)
                return new IntToken(source);
            if (type == TokenType.Double)
                return new DoubleToken(source);
            return new Token(source, type);
        }
    }
}
