using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MFL.Core.SyntaxParser;
using MFL.Core.TokenParser;

namespace MFL.Core.ExpTreeCompiler.BaseVisiter
{
    public abstract class BaseExpVisiter
    {
        protected ExpNode rootNode { get; set; }

        protected static Dictionary<TokenType, Func<Expression, Expression, BinaryExpression>> OpDic =
            new Dictionary<TokenType, Func<Expression, Expression, BinaryExpression>>()
            {
                {TokenType.AND, Expression.AndAlso},
                {TokenType.OR, Expression.OrElse},
                {TokenType.EQ, Expression.Equal},
                {TokenType.GE, Expression.GreaterThanOrEqual},
                {TokenType.GT, Expression.GreaterThan},
                {TokenType.LE, Expression.LessThanOrEqual},
                {TokenType.LL, Expression.LessThan},
            };


        public BaseExpVisiter(ExpNode root)
        {
            this.rootNode = root;
        }

        public virtual void StartVisit()
        {
            Visit(this.rootNode);
        }
        

        protected virtual void Visit(ExpNode node)
        {
            if (node is LogicExp)
            {
                Visit((LogicExp)node);
                return;
            }

            if (node is ComparrisonExp)
            {
                Visit((ComparrisonExp)node);
                return;
            }

            if (node is WithExp)
            {
                Visit((WithExp)node);
                return;
            }

            throw new Exception();
        }

        protected virtual void Visit(FieldExp fieldExp)
        {

        }

        protected virtual void Visit(GetFieldExp node)
        {
            if (!(node.ParentNode is FieldExp))
            {
                throw new Exception();
            }
            Visit((FieldExp)node.ParentNode);
            if (node.FieldNode is GetFieldExp)
            {
                Visit((GetFieldExp)node.FieldNode);
            }
            else if (node.FieldNode is FieldExp)
            {
                Visit((FieldExp)node.FieldNode);
            }
            else
            {
                throw new Exception();
            }
        }

        protected virtual void Visit(ConstExp exp)
        {

        }

        protected virtual void Visit(LogicExp logicExp)
        {
            var leftExp = logicExp.LeftNode;
            var rightExp = logicExp.RightNode;
            Expression leftExpression = null;
            Expression rightExpression = null;
            if (leftExp is LogicExp)
            {
                Visit((LogicExp)leftExp);
            }
            else if (leftExp is ComparrisonExp)
            {
                Visit((ComparrisonExp)leftExp);
            }
            else
            {
                throw new Exception();
            }

            if (rightExp is LogicExp)
            {
                Visit((LogicExp)rightExp);
            }
            else if (rightExp is ComparrisonExp)
            {
                Visit((ComparrisonExp)rightExp);
            }
            else
            {
                throw new Exception();
            }
        }

        protected virtual void Visit(ComparrisonExp compExp)
        {
            var leftExp = compExp.LeftNode;
            var rightExp = compExp.RightNode;
            if (!(leftExp is GetFieldExp))
                throw new Exception();
            if (!(rightExp is ConstExp) && !(rightExp is GetFieldExp))
                throw new Exception();
            Visit((GetFieldExp)leftExp);
            if (rightExp is ConstExp)
            {
                Visit((ConstExp)rightExp);
            }

            if (rightExp is GetFieldExp)
            {
                Visit((GetFieldExp)rightExp);
            }
        }

        protected virtual void Visit(WithExp withExp)
        {
            var logicOrComExp = withExp.LogicOrComExp;
            if (logicOrComExp is LogicExp)
                Visit((LogicExp)logicOrComExp);
            else if (logicOrComExp is ComparrisonExp)
                Visit((ComparrisonExp)logicOrComExp);
            else
                throw new Exception();
            foreach (var execFilter in withExp.ExeFilter)
            {
                Visit(execFilter);
            }
        }

        protected virtual void Visit(ExecuteFilterExp execExp)
        {
            Visit(execExp.GetfieldExp);
            Visit(execExp.FilterExp);
        }

        protected virtual void Visit(FilterExp filterExp)
        {
            Visit(filterExp.body);
        }
    }

    public class TypeVisiter<T> : BaseExpVisiter
        where T : class
    {

        private MContext context { get; set; }
        private Expression Expression = null;
        private Stack<ParameterExpression> rootParam = null;
        private static MethodInfo loopMethodInfo = null;
        protected static Dictionary<TokenType, Func<Expression, Expression, BinaryExpression>> OpDic =
            new Dictionary<TokenType, Func<Expression, Expression, BinaryExpression>>()
            {
                {TokenType.AND, Expression.AndAlso},
                {TokenType.OR, Expression.OrElse},
                {TokenType.EQ, Expression.Equal},
                {TokenType.GE, Expression.GreaterThanOrEqual},
                {TokenType.GT, Expression.GreaterThan},
                {TokenType.LE, Expression.LessThanOrEqual},
                {TokenType.LL, Expression.LessThan},
            };


        private Func<T, T> result = null;
        public Func<T, T> Result {
            get
            {
                if(result == null)
                    this.StartVisit();
                return result;
            }
        }
        
        static TypeVisiter()
        {
            loopMethodInfo = typeof(Template).GetMethod(nameof(Template.FilterItem));
        }

        public TypeVisiter(ExpNode node)
            : base(node)
        {
            rootParam = new Stack<ParameterExpression>();
            rootParam.Push(Expression.Parameter(typeof(T)));
            this.context = new MContext(typeof(T));
        }
        private Stack<Expression> temp = new Stack<Expression>();
        private ExpVisiter<string>.AccessInfo accessInfo = new ExpVisiter<string>.AccessInfo();
        protected override void Visit(ExpNode node)
        {
            base.Visit(node);
            temp.Push(Expression.Lambda(temp.Pop(), rootParam.Peek()));
        }

        protected override void Visit(LogicExp logicExp)
        {
            base.Visit(logicExp);
            var rightExp = temp.Pop();
            var leftExp = temp.Pop();
            temp.Push(OpDic[logicExp.BaseToken.OpType](leftExp, rightExp));
        }

        protected override void Visit(ComparrisonExp compExp)
        {
            base.Visit(compExp);
            var exp = temp.Pop();
            var leftAccess = accessInfo;
            if (exp.GetType() == typeof(ConstantExpression))
            {
                if(((ConstantExpression)exp).Type != leftAccess.FieldType)
                    throw new Exception();
            }
            else
            {
                //todo 暂时不支持对比表达式右边是取字段或者字段操作
                throw new Exception();
            }
            var creator = OpDic[compExp.BaseToken.OpType];
            creator(leftAccess.exp, exp);
        }

        protected override void Visit(ConstExp exp)
        {
            if (exp.BaseToken.OpType == TokenType.Int)
            {
                var tempVal = int.Parse(exp.BaseToken.Source);
                temp.Push(Expression.Constant(tempVal, typeof(int)));
            }

            if (exp.BaseToken.OpType == TokenType.Double)
            {
                var tempVal = double.Parse(exp.BaseToken.Source);
                temp.Push(Expression.Constant(tempVal, typeof(double)));
            }

            if (exp.BaseToken.OpType == TokenType.ConstStr)
            {
                var tempVal = exp.BaseToken.Source;
                temp.Push(Expression.Constant(tempVal, typeof(string)));
            }
            throw new Exception();
        }

        protected override void Visit(GetFieldExp node)
        {
            base.Visit(node);
        }

        private Stack<Tuple<string, Type>> scopStack = new Stack<Tuple<string, Type>>();
        protected override void Visit(FieldExp fieldExp)
        {
            var fieldToken = fieldExp.BaseToken;
            var curContent = context.Current;
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
    }
}
