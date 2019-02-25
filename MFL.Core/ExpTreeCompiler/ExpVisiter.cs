using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MFL.Core.SyntaxParser;
using MFL.Core.TokenParser;

namespace MFL.Core.ExpTreeCompiler
{

    public class ExpVisiter<T> where T : class
    {
        private MContext context { get; set; }
        private ExpNode rootNode { get; set; }
        private Expression Expression = null;
        private Stack<ParameterExpression> rootParam = null;
        private static MethodInfo loopMethodInfo = null;

        static ExpVisiter()
        {
            loopMethodInfo = typeof(Template).GetMethod(nameof(Template.FilterItem));
        }

        protected static Dictionary<TokenType, Func<Expression, Expression, Expression>> OpDic =
            new Dictionary<TokenType, Func<Expression, Expression, Expression>>()
            {
                {TokenType.AND, Expression.AndAlso},
                {TokenType.OR, Expression.OrElse},
                {TokenType.EQ, Template.Eq},
                {TokenType.GE, Expression.GreaterThanOrEqual},
                {TokenType.GT, Expression.GreaterThan},
                {TokenType.LE, Expression.LessThanOrEqual},
                {TokenType.LL, Expression.LessThan},
            };


        public ExpVisiter(ExpNode root)
        {
            rootParam = new Stack<ParameterExpression>();
            rootParam.Push(Expression.Parameter(typeof(T)));
            this.context = new MContext(typeof(T));
            this.rootNode = root;
        }

        public Func<T, T> StartVisit()
        {
            var invokerExp = Visit(this.rootNode);
            var invoker = (Func<T, bool>)invokerExp.Compile();
            return x => invoker(x) ? x : null;
        }

        protected virtual LambdaExpression Visit(ExpNode node)
        {
            if (node is LogicExp)
            {
                var body = Visit((LogicExp)node);
                return Expression.Lambda(body, rootParam.Peek());
            }

            if (node is ComparrisonExp)
            {
                var body = Visit((ComparrisonExp)node);
                return Expression.Lambda(body, rootParam.Peek());
            }

            if (node is WithExp)
            {
                var body = Visit((WithExp)node);
                return Expression.Lambda(body, rootParam.Peek());
            }

            throw new Exception();
        }

        protected virtual void Visit(FieldExp fieldExp, Stack<Tuple<string, Type>> scopStack, AccessInfo accessInfo)
        {
            var fieldToken = fieldExp.BaseToken;
            if (fieldToken.OpType != TokenType.Field)
                throw new Exception();
            if (scopStack.Count == 0)
            {
                if (!fieldToken.Source.Equals(context.Current.Item1, StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new Exception();
                }
                scopStack.Push(context.Current);
                accessInfo.exp = rootParam.Peek();
                accessInfo.FieldType = rootParam.Peek().Type;
            }
            else
            {
                var temp = scopStack.Peek();
                var property = temp.Item2.GetProperty(fieldToken.Source, BindingFlags.Instance | BindingFlags.Public);
                MemberInfo meminfo = null;
                if (property == null)
                {
                    var fieldInfo = temp.Item2.GetField(fieldToken.Source, BindingFlags.Instance | BindingFlags.Public);
                    if (fieldInfo == null)
                        throw new Exception();
                    var ftype = fieldInfo.FieldType;
                    scopStack.Push(new Tuple<string, Type>(fieldToken.Source, ftype));
                    meminfo = fieldInfo;
                    accessInfo.FieldType = fieldInfo.FieldType;
                }
                else
                {
                    var ptype = property.PropertyType;
                    scopStack.Push(new Tuple<string, Type>(fieldToken.Source, ptype));
                    meminfo = property;
                    accessInfo.FieldType = property.PropertyType;
                }

                accessInfo.exp = Expression.MakeMemberAccess(accessInfo.exp, meminfo);
            }
        }

        protected virtual void Visit(GetFieldExp node, Stack<Tuple<string, Type>> scopStack, AccessInfo accessInfo)
        {
            if (!(node.ParentNode is FieldExp))
            {
                throw new Exception();
            }
            Visit((FieldExp)node.ParentNode, scopStack, accessInfo);
            if (node.FieldNode is GetFieldExp)
            {
                Visit((GetFieldExp)node.FieldNode, scopStack, accessInfo);
            }
            else if (node.FieldNode is FieldExp)
            {
                Visit((FieldExp)node.FieldNode, scopStack, accessInfo);
            }
            else
            {
                throw new Exception();
            }
        }

        protected virtual Tuple<Type, Object> Visit(ConstExp exp)
        {
            if (exp.BaseToken.OpType == TokenType.Int)
            {
                return new Tuple<Type, object>(typeof(int), int.Parse(exp.BaseToken.Source));
            }

            if (exp.BaseToken.OpType == TokenType.Double)
            {
                return new Tuple<Type, object>(typeof(double), double.Parse(exp.BaseToken.Source));
            }

            if (exp.BaseToken.OpType == TokenType.ConstStr)
            {
                return new Tuple<Type, object>(typeof(string), exp.BaseToken.Source);
            }
            throw new Exception();
        }

        protected virtual Expression Visit(LogicExp logicExp)
        {
            var leftExp = logicExp.LeftNode;
            var rightExp = logicExp.RightNode;
            Expression leftExpression = null;
            Expression rightExpression = null;
            if (leftExp is LogicExp)
            {
                leftExpression = Visit((LogicExp)leftExp);
            }
            else if (leftExp is ComparrisonExp)
            {
                leftExpression = Visit((ComparrisonExp)leftExp);
            }
            else
            {
                throw new Exception();
            }

            if (rightExp is LogicExp)
            {
                rightExpression = Visit((LogicExp)rightExp);
            }
            else if (rightExp is ComparrisonExp)
            {
                rightExpression = Visit((ComparrisonExp)rightExp);
            }
            else
            {
                throw new Exception();
            }

            return OpDic[logicExp.BaseToken.OpType](leftExpression, rightExpression);
        }

        protected virtual Expression Visit(ComparrisonExp compExp)
        {
            var leftExp = compExp.LeftNode;
            var rightExp = compExp.RightNode;
            if (!(leftExp is GetFieldExp))
                throw new Exception();
            if (!(rightExp is ConstExp) && !(rightExp is GetFieldExp))
                throw new Exception();
            var leftAccess = new AccessInfo();
            Visit((GetFieldExp)leftExp, new Stack<Tuple<string, Type>>(), leftAccess);
            Expression right = null;
            Tuple<Type, object> constInfo = null;
            if (rightExp is ConstExp)
            {
                constInfo = Visit((ConstExp)rightExp);
                //类型校验
                if (constInfo.Item1 != leftAccess.FieldType)
                    throw new Exception();
                right = Expression.Constant(constInfo.Item2);
            }

            if (rightExp is GetFieldExp)
            {
                AccessInfo rightAccess = new AccessInfo();
                Visit((GetFieldExp)rightExp, new Stack<Tuple<string, Type>>(), rightAccess);
                if (leftAccess.FieldType != rightAccess.FieldType)
                    throw new Exception();
                //todo 暂时不支持对比表达式右边是取字段或者字段操作
                throw new Exception();
            }

            var creator = OpDic[compExp.BaseToken.OpType];
            return creator(leftAccess.exp, right);
        }

        protected virtual Expression Visit(WithExp withExp)
        {
            var logicOrComExp = withExp.LogicOrComExp;
            Expression exp = null;
            if (logicOrComExp is LogicExp)
                exp = Visit((LogicExp)logicOrComExp);
            else if (logicOrComExp is ComparrisonExp)
                exp = Visit((ComparrisonExp)logicOrComExp);
            else if (logicOrComExp == null)
                exp = Expression.Constant(true);
            else
                throw new Exception();
            foreach (var execFilter in withExp.ExeFilter)
            {
                var funcMethod = Visit(execFilter);
                exp = Expression.And(exp, funcMethod);
            }
            return exp;
        }

        protected virtual MethodCallExpression Visit(ExecuteFilterExp execExp)
        {
            AccessInfo info = new AccessInfo();
            var stack = new Stack<Tuple<string, Type>>();
            Visit(execExp.GetfieldExp, stack, info);
            var accessFieldExp = info.exp;
            if (info.FieldType != stack.Peek().Item2)
                throw new Exception();
            if (!info.FieldType.IsGenericType || !(info.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                throw new Exception();

            var method = Visit(execExp.FilterExp, info.FieldType.GetGenericArguments()[0]);
            var thisLoopMethodInfo = loopMethodInfo.MakeGenericMethod(info.FieldType.GetGenericArguments()[0]);
            var exp = Expression.Call(thisLoopMethodInfo, accessFieldExp, Expression.Constant(method));
            var tempDelegate = Expression.Lambda(exp, rootParam.Peek()).Compile();
            var methodwar = typeof(Template).GetMethod(nameof(Template.F));
            var realMethodWar = methodwar.MakeGenericMethod(rootParam.Peek().Type);
            return Expression.Call(realMethodWar, rootParam.Peek(), Expression.Constant(tempDelegate));
        }

        protected virtual Delegate Visit(FilterExp filterExp, Type newRootType)
        {
            var paramname = filterExp.fieldExp.BaseToken.Source;
            context.Set(newRootType, paramname);
            rootParam.Push(Expression.Parameter(newRootType, paramname));
            var filter = Visit(filterExp.body);
            context.Get();
            rootParam.Pop();
            return filter.Compile();
        }



        public class AccessInfo
        {
            public Type FieldType { get; set; }
            public Expression exp { get; set; }
        }
    }
}
