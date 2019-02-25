using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFL.Core.TokenParser;

namespace MFL.Core.SyntaxParser
{
    public static class SParser
    {
        //运算符优先级和结合性表
        static Dictionary<TokenType, Tuple<int, Combination>> priority = new Dictionary<TokenType, Tuple<int, Combination>>()
        {
            { TokenType.DotOp, new Tuple<int, Combination>(1000, Combination.RIGHT) },
            { TokenType.FilterStart, new Tuple<int, Combination>(0, Combination.NON) },
            { TokenType.FilterEnd, new Tuple<int, Combination>(999, Combination.NON) },
            { TokenType.FilterOP, new Tuple<int, Combination>(975, Combination.RIGHT) },
            { TokenType.AND, new Tuple<int, Combination>(990, Combination.LEFT) },
            { TokenType.OR, new Tuple<int, Combination>(980, Combination.LEFT) },
            { TokenType.WITH, new Tuple<int, Combination>(978, Combination.LEFT) },
            { TokenType.LINK, new Tuple<int, Combination>(977, Combination.LEFT) },
            { TokenType.EQ, new Tuple<int, Combination>(995, Combination.NON) },
            { TokenType.GT, new Tuple<int, Combination>(995, Combination.NON) },
            { TokenType.GE, new Tuple<int, Combination>(995, Combination.NON) },
            { TokenType.LE, new Tuple<int, Combination>(995, Combination.NON) },
            { TokenType.LL, new Tuple<int, Combination>(995, Combination.NON) },
            { TokenType.Begin, new Tuple<int, Combination>(0, Combination.NON) },
            { TokenType.End, new Tuple<int, Combination>(1, Combination.NON) },
        };

        //结合性
        enum Combination
        {
            LEFT,
            RIGHT,
            NON
        }
        public static ExpNode CreateExpTree(this List<Token> tokenList)
        {
            tokenList.Add(Token.END);
            Stack<ExpNode> expStack = new Stack<ExpNode>();
            Stack<Stack<ExpNode>> expStackStack = new Stack<Stack<ExpNode>>();
            Stack<Token> verbStack = new Stack<Token>();
            Stack<Stack<Token>> verbStackStack = new Stack<Stack<Token>>();
            verbStack.Push(Token.BEGIN);
            for (int i = 0; i < tokenList.Count; i++)
            {
                Token token = tokenList[i];
                if (token.OpType == TokenType.Field)
                {
                    var temp = new FieldExp(token);
                    expStack.Push(temp);
                    continue;
                }

                if (token.OpType == TokenType.ConstStr || token.OpType == TokenType.Int || token.OpType == TokenType.Double)
                {
                    var temp = new ConstExp(token);
                    expStack.Push(temp);
                    continue;
                }

                if (token.OpType == TokenType.FilterStart)
                {
                    expStackStack.Push(expStack);
                    verbStackStack.Push(verbStack);
                    expStack = new Stack<ExpNode>();
                    verbStack = new Stack<Token>();
                    verbStack.Push(token);
                    continue;
                }

                if (token.OpType == TokenType.FilterEnd)
                {
                    while (verbStack.Peek().OpType != TokenType.FilterStart)
                    {
                        CreateNode(expStack, verbStack.Pop());
                    }
                    if (expStack.Count != 1)
                        throw new Exception();
                    var subExp = expStack.Pop();
                    if (!(subExp is FilterExp))
                    {
                        throw new Exception();
                    }

                    FilterExp subExpFilter = (FilterExp)subExp;
                    expStack = expStackStack.Pop();
                    verbStack = verbStackStack.Pop();
                    if (!priority.TryGetValue(token.OpType, out var endPriority))
                        throw new Exception();
                    while (true)
                    {
                        Token topToken = verbStack.Peek();
                        var topinfo = priority[topToken.OpType];
                        if (endPriority.Item1 > topinfo.Item1)
                            break;
                        CreateNode(expStack, verbStack.Pop());
                    }
                    var getFieldExp = expStack.Pop();
                    if (!(getFieldExp is GetFieldExp))
                        throw new Exception();
                    expStack.Push(new ExecuteFilterExp((GetFieldExp)getFieldExp, subExpFilter));
                    continue;
                }

                if (priority.TryGetValue(token.OpType, out var nowinfo))
                {
                    while (true)
                    {
                        Token topToken = verbStack.Peek();
                        var topinfo = priority[topToken.OpType];
                        if (topinfo.Item1 == nowinfo.Item1)
                        {
                            if (topinfo.Item2 == Combination.LEFT)
                            {
                                CreateNode(expStack, verbStack.Pop());
                            }

                            if (topinfo.Item2 == Combination.RIGHT)
                            {
                                verbStack.Push(token);
                                break;
                            }

                            if (topinfo.Item2 == Combination.NON)
                                throw new Exception();
                            continue;
                        }
                        if (topinfo.Item1 > nowinfo.Item1)
                        {
                            CreateNode(expStack, verbStack.Pop());
                        }
                        if (topinfo.Item1 < nowinfo.Item1)
                        {
                            verbStack.Push(token);
                            break;
                        }
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            //判断
            if (verbStackStack.Count != 0 || expStackStack.Count != 0)
            {
                throw new Exception();
            }

            if (verbStack.Count == 2 && expStack.Count == 1 && verbStack.Pop().OpType == TokenType.End &&
                verbStack.Pop().OpType == TokenType.Begin)
                return expStack.Pop();
            throw new Exception();
        }


        static void CreateNode(Stack<ExpNode> expStack, Token token)
        {
            if (token.OpType == TokenType.DotOp)
            {
                var expRight = expStack.Pop();
                var expLeft = expStack.Pop();
                var expTemp = new GetFieldExp(token, expLeft, expRight);
                expStack.Push(expTemp);
                return;
            }

            if (token.OpType == TokenType.AND
                || token.OpType == TokenType.OR)
            {
                var expRight = expStack.Pop();
                var expLeft = expStack.Pop();
                var expTemp = new LogicExp(token, expLeft, expRight);
                expStack.Push(expTemp);
                return;
            }

            if (token.OpType == TokenType.EQ
                || token.OpType == TokenType.GE
                || token.OpType == TokenType.GT
                || token.OpType == TokenType.LL
                || token.OpType == TokenType.LE)
            {
                var expRight = expStack.Pop();
                var expLeft = expStack.Pop();
                var expTemp = new ComparrisonExp(token, expLeft, expRight);
                expStack.Push(expTemp);
                return;
            }

            if (token.OpType == TokenType.FilterOP)
            {
                var body = expStack.Pop();
                var head = expStack.Pop();
                expStack.Push(new FilterExp(body, head));
                return;
            }

            if (token.OpType == TokenType.WITH)
            {
                var withStatement = expStack.Pop();
                if (!(withStatement is ExecuteFilterExp))
                    throw new Exception();
                if (expStack.Peek() is WithExp)
                {
                    ((WithExp)expStack.Peek()).AddWith((ExecuteFilterExp)withStatement);
                }
                else
                {
                    var mainExp = expStack.Pop();
                    if (mainExp is LogicExp)
                    {
                        expStack.Push(new WithExp((LogicExp)mainExp, (ExecuteFilterExp)withStatement));
                    }
                    else if (mainExp is ComparrisonExp)
                    {
                        expStack.Push(new WithExp((ComparrisonExp)mainExp, (ExecuteFilterExp)withStatement));
                    }
                    //当仅有with表达式,没有前置的对比和逻辑表达式的时候
                    else if (mainExp is FieldExp)
                    {
                        expStack.Push(mainExp);
                        expStack.Push(new WithExp(null, (ExecuteFilterExp)withStatement));
                    }
                    else
                    {
                        throw new Exception();
                    }
                    /*
                    if(!(mainExp is LogicExp) && !(mainExp is ComparrisonExp))
                        throw new Exception();
                    expStack.Push(new WithExp((LogicExp)mainExp, (ExecuteFilterExp)withStatement));
                    */
                }
                return;
            }

            if (token.OpType == TokenType.LINK)
            {
                var withStatement = expStack.Pop();
                if (!(withStatement is ExecuteFilterExp))
                    throw new Exception();
                if (!(expStack.Peek() is WithExp))
                {
                    throw new Exception();
                }
                ((WithExp)expStack.Peek()).AddWith((ExecuteFilterExp)withStatement);
                return;
            }

            throw new Exception();

        }
    }
}
