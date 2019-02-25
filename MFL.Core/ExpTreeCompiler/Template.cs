using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MFL.Core.ExpTreeCompiler
{
    public class Template
    {
        private static MethodInfo StartWith = null;
        private static MethodInfo EndWith = null;
        private static MethodInfo Contant = null;
        static Template()
        {
            var argsType = new Type[] {typeof(string)};
            StartWith = typeof(String).GetMethod(nameof(String.StartsWith), argsType);
            EndWith = typeof(String).GetMethod(nameof(String.EndsWith), argsType);
            Contant = typeof(String).GetMethod(nameof(String.Contains), argsType);
        }

        public static bool FilterItem<Y>(List<Y> source, Func<Y, bool> filter)
        {
            if (source == null)
                return true;
            Console.WriteLine(typeof(Y).FullName);
            List<Y> remItemList = new List<Y>();
            foreach (Y item in source)
            {
                if (!filter(item))
                    remItemList.Add(item);
            }

            foreach (var remItem in remItemList)
            {
                source.Remove(remItem);
            }

            return true;
        }

        public static bool F<T>(T source, Func<T, bool> worker)
        {
            Console.WriteLine(typeof(T).FullName);
            return worker(source);
        }

        public static Expression Eq(Expression left, Expression right)
        {
            Expression result = null;
            if (left.Type == typeof(string))
            {
                var constExp = ((ConstantExpression) right);
                var val = (string) constExp.Value;
                var isStartLike = val.StartsWith("%");
                if (isStartLike)
                {
                    val = val.Substring(1);
                }

                var isEndLike = val.EndsWith("%");
                if (isEndLike)
                {
                    val = val.Substring(0, val.Length - 1);
                }

                if(isStartLike || isEndLike)
                    right = Expression.Constant(val);
                if (isStartLike && isEndLike)
                {
                    result = Expression.Call(left, Contant, right);
                }
                else if (isStartLike)
                {
                    result = Expression.Call(left, StartWith, right);
                }
                else if(isEndLike)
                {
                    result = Expression.Call(left, EndWith, right);
                }
                else
                {
                    result = Expression.Equal(left, right);
                }
            }
            else
            {
                result = Expression.Equal(left, right);
            }

            return result;
        }
    }
}
